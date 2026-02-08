using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Domain.Entities.Etl;

namespace RentWisePro.Web.Services.Etl;

public class LocalEtlControlService : IEtlControlService
{
    private const int RunnerLockMinutes = 30;
    private const int MaxMessageLength = 4000;
    private static readonly SemaphoreSlim RunnerLock = new(1, 1);

    private readonly EtlReadDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalEtlControlService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public LocalEtlControlService(
        EtlReadDbContext dbContext,
        IWebHostEnvironment environment,
        ILogger<LocalEtlControlService> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _dbContext = dbContext;
        _environment = environment;
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    public Task<EtlActionResult> TriggerIngestionRunOnceAsync(string? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return TriggerRunnerAsync(
            "IngestionRunOnce",
            new[] { "--runOnce" },
            requestedByUserId,
            cancellationToken);
    }

    public Task<EtlActionResult> TriggerQueueRunOnceAsync(string? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return TriggerRunnerAsync(
            "QueueRunOnce",
            new[] { "--queue-only", "--queue-once" },
            requestedByUserId,
            cancellationToken);
    }

    public Task<EtlActionResult> DisableLocalScheduleAsync(string? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return RunScheduleScriptAsync(
            "DisableSchedule",
            "unregister-tasks.ps1",
            requestedByUserId,
            cancellationToken);
    }

    public Task<EtlActionResult> EnableLocalScheduleAsync(string? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return RunScheduleScriptAsync(
            "EnableSchedule",
            "register-tasks.ps1",
            requestedByUserId,
            cancellationToken);
    }

    public async Task<EtlRunnerStatus> GetRunnerStatusAsync(CancellationToken cancellationToken = default)
    {
        var runningAction = await HasRecentRunningActionAsync(cancellationToken);
        var runningRun = await _dbContext.EtlRuns
            .AnyAsync(run => run.Status == "Running" && run.StartedAt >= DateTimeOffset.UtcNow.AddMinutes(-RunnerLockMinutes), cancellationToken);

        var isRunning = runningAction || runningRun;
        var statusMessage = isRunning
            ? "ETL is currently running or was started recently."
            : "No ETL run is currently reported as running.";

        var scheduleStatus = await GetScheduleStatusAsync(cancellationToken);

        return new EtlRunnerStatus(isRunning, statusMessage, scheduleStatus.Enabled, scheduleStatus.Message);
    }

    private async Task<EtlActionResult> TriggerRunnerAsync(
        string actionType,
        IReadOnlyList<string> arguments,
        string? requestedByUserId,
        CancellationToken cancellationToken)
    {
        if (!await RunnerLock.WaitAsync(0, cancellationToken))
        {
            return new EtlActionResult(false, "Another ETL action is currently starting. Please wait a moment and try again.");
        }

        try
        {
            if (await HasRecentRunningActionAsync(cancellationToken) || await HasRecentRunningRunAsync(cancellationToken))
            {
                return new EtlActionResult(false, "ETL already appears to be running. Wait for the current run to finish before starting another.");
            }

            var command = ResolveEtlCommand(arguments, out var commandArguments, out var failureMessage);
            if (command is null)
            {
                return await CreateFailedActionAsync(actionType, requestedByUserId, failureMessage ?? "Unable to resolve ETL command.", cancellationToken);
            }

            var action = await CreateRunningActionAsync(actionType, requestedByUserId, command, cancellationToken);

            try
            {
                var process = StartProcess(command, commandArguments);
                if (process is null)
                {
                    return await UpdateActionFailureAsync(action, "Failed to start the ETL process.", cancellationToken);
                }

                _ = Task.Run(async () =>
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    var message = BuildMessage(output, error);
                    var status = process.ExitCode == 0 ? "Succeeded" : "Failed";
                    await UpdateActionFromBackgroundAsync(action.ActionId, status, message);
                }, CancellationToken.None);

                return new EtlActionResult(true, "ETL run started. Monitor the status below for completion.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start ETL process for {ActionType}.", actionType);
                return await UpdateActionFailureAsync(action, "Failed to start ETL process. See logs for details.", cancellationToken);
            }
        }
        finally
        {
            RunnerLock.Release();
        }
    }

    private async Task<EtlActionResult> RunScheduleScriptAsync(
        string actionType,
        string scriptName,
        string? requestedByUserId,
        CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            return await CreateFailedActionAsync(actionType, requestedByUserId, "Schedule toggles are only supported on Windows.", cancellationToken);
        }

        var scriptPath = Path.Combine(_environment.ContentRootPath, "scripts", "etl", scriptName);
        if (!File.Exists(scriptPath))
        {
            return await CreateFailedActionAsync(actionType, requestedByUserId, $"Script not found: {scriptPath}", cancellationToken);
        }

        var action = await CreateRunningActionAsync(actionType, requestedByUserId, $"powershell.exe -File {scriptPath}", cancellationToken);

        try
        {
            var args = BuildPowerShellArguments(scriptPath);
            var process = StartProcess("powershell.exe", args);
            if (process is null)
            {
                return await UpdateActionFailureAsync(action, "Failed to start PowerShell.", cancellationToken);
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(cancellationToken);

            var message = BuildMessage(output, error);
            var status = process.ExitCode == 0 ? "Succeeded" : "Failed";
            var fallbackMessage = process.ExitCode == 0
                ? "Schedule updated successfully."
                : "Schedule update failed.";
            var finalMessage = string.IsNullOrWhiteSpace(message) ? fallbackMessage : message;
            await UpdateActionAsync(action, status, finalMessage, cancellationToken);

            return new EtlActionResult(process.ExitCode == 0, finalMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run schedule script {ScriptName}.", scriptName);
            return await UpdateActionFailureAsync(action, "Failed to update schedule. See logs for details.", cancellationToken);
        }
    }

    private string? ResolveEtlCommand(IReadOnlyList<string> arguments, out string commandArguments, out string? failureMessage)
    {
        var projectPath = Path.Combine(_environment.ContentRootPath, "Etl", "RentWisePro.Etl.csproj");
        if (!File.Exists(projectPath))
        {
            commandArguments = string.Empty;
            failureMessage = $"ETL project file not found at {projectPath}.";
            return null;
        }

        var releaseRoot = Path.Combine(_environment.ContentRootPath, "Etl", "bin", "Release", "net8.0");
        var exePath = OperatingSystem.IsWindows()
            ? Path.Combine(releaseRoot, "RentWisePro.Etl.exe")
            : Path.Combine(releaseRoot, "RentWisePro.Etl");
        var dllPath = Path.Combine(releaseRoot, "RentWisePro.Etl.dll");

        if (File.Exists(exePath))
        {
            commandArguments = string.Join(' ', arguments);
            failureMessage = null;
            return exePath;
        }

        if (File.Exists(dllPath))
        {
            commandArguments = $"{Quote(dllPath)} {string.Join(' ', arguments)}";
            failureMessage = null;
            return "dotnet";
        }

        commandArguments = $"run --project {Quote(projectPath)} -- {string.Join(' ', arguments)}";
        failureMessage = null;
        return "dotnet";
    }

    private Process? StartProcess(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = _environment.ContentRootPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.Environment["DOTNET_ENVIRONMENT"] = _environment.EnvironmentName;

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        return process.Start() ? process : null;
    }

    private async Task<bool> HasRecentRunningActionAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-RunnerLockMinutes);
        return await _dbContext.EtlAdminActions
            .AnyAsync(action => action.Status == "Running" && action.StartedAt >= cutoff, cancellationToken);
    }

    private async Task<bool> HasRecentRunningRunAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-RunnerLockMinutes);
        return await _dbContext.EtlRuns
            .AnyAsync(run => run.Status == "Running" && run.StartedAt >= cutoff, cancellationToken);
    }

    private async Task<EtlAdminAction> CreateRunningActionAsync(
        string actionType,
        string? requestedByUserId,
        string command,
        CancellationToken cancellationToken)
    {
        var action = new EtlAdminAction
        {
            ActionId = Guid.NewGuid(),
            ActionType = actionType,
            StartedAt = DateTimeOffset.UtcNow,
            Status = "Running",
            RequestedByUserId = requestedByUserId,
            Command = command
        };

        _dbContext.EtlAdminActions.Add(action);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return action;
    }

    private async Task<EtlActionResult> CreateFailedActionAsync(
        string actionType,
        string? requestedByUserId,
        string message,
        CancellationToken cancellationToken)
    {
        var action = new EtlAdminAction
        {
            ActionId = Guid.NewGuid(),
            ActionType = actionType,
            StartedAt = DateTimeOffset.UtcNow,
            FinishedAt = DateTimeOffset.UtcNow,
            Status = "Failed",
            RequestedByUserId = requestedByUserId,
            Message = message
        };

        _dbContext.EtlAdminActions.Add(action);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new EtlActionResult(false, message);
    }

    private async Task<EtlActionResult> UpdateActionFailureAsync(
        EtlAdminAction action,
        string message,
        CancellationToken cancellationToken)
    {
        await UpdateActionAsync(action, "Failed", message, cancellationToken);
        return new EtlActionResult(false, message);
    }

    private async Task UpdateActionAsync(
        EtlAdminAction action,
        string status,
        string message,
        CancellationToken cancellationToken)
    {
        action.Status = status;
        action.FinishedAt = DateTimeOffset.UtcNow;
        action.Message = message;
        _dbContext.EtlAdminActions.Update(action);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateActionFromBackgroundAsync(Guid actionId, string status, string message)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EtlReadDbContext>();
            var action = await dbContext.EtlAdminActions.FirstOrDefaultAsync(item => item.ActionId == actionId);
            if (action is null)
            {
                return;
            }

            action.Status = status;
            action.FinishedAt = DateTimeOffset.UtcNow;
            action.Message = message;
            dbContext.EtlAdminActions.Update(action);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update ETL admin action {ActionId}.", actionId);
        }
    }

    private async Task<(bool? Enabled, string? Message)> GetScheduleStatusAsync(CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsWindows())
        {
            return (null, "Schedule status is only available on Windows.");
        }

        try
        {
            var ingestionExists = await IsTaskRegisteredAsync("RentWisePro-ETL-Ingestion", cancellationToken);
            var queueExists = await IsTaskRegisteredAsync("RentWisePro-ETL-Queue", cancellationToken);

            var enabled = ingestionExists && queueExists;
            var message = enabled
                ? "Scheduled tasks are registered."
                : "Scheduled tasks are not registered.";

            return (enabled, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query schedule status.");
            return (null, "Unable to query schedule status. See logs for details.");
        }
    }

    private async Task<bool> IsTaskRegisteredAsync(string taskName, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "schtasks",
            Arguments = $"/Query /TN {Quote(taskName)}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode == 0;
    }

    private string BuildPowerShellArguments(string scriptPath)
    {
        var arguments = new List<string>
        {
            "-NoProfile",
            "-ExecutionPolicy", "Bypass",
            "-File", Quote(scriptPath),
            "-Environment", Quote(_environment.EnvironmentName),
            "-ProjectPath", Quote("Etl/RentWisePro.Etl.csproj")
        };

        var connectionString = _configuration.GetConnectionString("RentWiseProDb");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            arguments.Add("-ConnectionString");
            arguments.Add(Quote(connectionString));
        }

        return string.Join(' ', arguments);
    }

    private static string BuildMessage(string output, string error)
    {
        var combined = string.Join(Environment.NewLine, new[] { output, error }.Where(value => !string.IsNullOrWhiteSpace(value)));
        if (combined.Length <= MaxMessageLength)
        {
            return combined;
        }

        return combined[..MaxMessageLength] + "...";
    }

    private static string Quote(string value)
    {
        return $"\"{value}\"";
    }
}
