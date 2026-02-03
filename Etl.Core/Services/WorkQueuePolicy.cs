namespace RentWisePro.Etl.Core.Services;

public static class WorkQueuePolicy
{
    public static bool IsClaimable(string status, DateTimeOffset availableAt, DateTimeOffset now)
    {
        return string.Equals(status, "queued", StringComparison.OrdinalIgnoreCase) && availableAt <= now;
    }

    public static DateTimeOffset CalculateNextAvailable(DateTimeOffset now, int attempts)
    {
        var delayMinutes = Math.Min(Math.Pow(2, attempts), 60);
        return now.AddMinutes(delayMinutes);
    }
}
