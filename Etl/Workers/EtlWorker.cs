using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Options;

namespace RentWisePro.Etl.Workers;

public class EtlWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EtlExecutionOptions _options;
    private readonly ILogger<EtlWorker> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public EtlWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<EtlExecutionOptions> options,
        ILogger<EtlWorker> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.QueueOnly)
        {
            _logger.LogInformation("Queue-only mode enabled. Skipping orchestrator runs.");
            return;
        }

        _logger.LogInformation(
            "ETL ingestion configured (runOnce={RunOnce}, interval={Interval}, source={SourceFilter}, since={Since}, pageSize={PageSize})",
            _options.RunOnce,
            _options.Interval,
            _options.SourceFilter ?? "all",
            _options.Since?.ToString("O") ?? "unspecified",
            _options.PageSize);

        if (_options.RunOnce)
        {
            await RunOnceAsync(stoppingToken);
            _applicationLifetime.StopApplication();
            return;
        }

        using var timer = new PeriodicTimer(_options.Interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting ETL run (source={SourceFilter})", _options.SourceFilter ?? "all");
        using var scope = _scopeFactory.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IEtlOrchestrator>();
        await orchestrator.RunAsync(new EtlRunRequest(_options.SourceFilter, _options.Since, _options.PageSize), stoppingToken);
    }
}
