namespace RentWisePro.Web.Services.Etl;

public interface IEtlControlService
{
    Task<EtlActionResult> TriggerIngestionRunOnceAsync(string? requestedByUserId, CancellationToken cancellationToken = default);
    Task<EtlActionResult> TriggerQueueRunOnceAsync(string? requestedByUserId, CancellationToken cancellationToken = default);
    Task<EtlActionResult> DisableLocalScheduleAsync(string? requestedByUserId, CancellationToken cancellationToken = default);
    Task<EtlActionResult> EnableLocalScheduleAsync(string? requestedByUserId, CancellationToken cancellationToken = default);
    Task<EtlRunnerStatus> GetRunnerStatusAsync(CancellationToken cancellationToken = default);
}

public record EtlActionResult(bool Success, string Message);

public record EtlRunnerStatus(bool IsRunning, string StatusMessage, bool? ScheduleEnabled, string? ScheduleStatusMessage);
