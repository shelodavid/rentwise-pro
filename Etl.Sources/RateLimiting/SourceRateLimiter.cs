namespace RentWisePro.Etl.Sources.RateLimiting;

public class SourceRateLimiter
{
    private readonly SemaphoreSlim _semaphore;
    private readonly TimeSpan _minInterval;
    private readonly object _lock = new();
    private DateTimeOffset _lastRequest = DateTimeOffset.MinValue;

    public SourceRateLimiter(int maxConcurrency, int maxRequestsPerMinute)
    {
        _semaphore = new SemaphoreSlim(Math.Max(1, maxConcurrency));
        var rate = Math.Max(1, maxRequestsPerMinute);
        _minInterval = TimeSpan.FromMinutes(1) / rate;
    }

    public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        await ThrottleAsync(cancellationToken);
        return new Releaser(_semaphore);
    }

    private Task ThrottleAsync(CancellationToken cancellationToken)
    {
        TimeSpan delay;
        lock (_lock)
        {
            var now = DateTimeOffset.UtcNow;
            var next = _lastRequest + _minInterval;
            delay = next > now ? next - now : TimeSpan.Zero;
            _lastRequest = now + delay;
        }

        return delay > TimeSpan.Zero ? Task.Delay(delay, cancellationToken) : Task.CompletedTask;
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim;

        public Releaser(SemaphoreSlim semaphoreSlim)
        {
            _semaphoreSlim = semaphoreSlim;
        }

        public void Dispose()
        {
            _semaphoreSlim.Release();
        }
    }
}
