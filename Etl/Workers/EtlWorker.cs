using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Core.Services;
using RentWisePro.Etl.Options;

namespace RentWisePro.Etl.Workers;

public class EtlWorker : BackgroundService
{
    private readonly EtlOrchestrator _orchestrator;
    private readonly EtlExecutionOptions _options;
    private readonly ILogger<EtlWorker> _logger;

    public EtlWorker(
        EtlOrchestrator orchestrator,
        IOptions<EtlExecutionOptions> options,
        ILogger<EtlWorker> logger)
    {
        _orchestrator = orchestrator;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.QueueOnly)
        {
            _logger.LogInformation("Queue-only mode enabled. Skipping orchestrator runs.");
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            return;
        }

        if (_options.RunOnce)
        {
            await RunOnceAsync(stoppingToken);
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
        await _orchestrator.RunAsync(new EtlRunRequest(_options.SourceFilter, _options.Since, _options.PageSize), stoppingToken);
    }
}
