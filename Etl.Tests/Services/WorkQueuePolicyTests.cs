using RentWisePro.Etl.Core.Services;

namespace RentWisePro.Etl.Tests.Services;

public class WorkQueuePolicyTests
{
    [Fact]
    public void IsClaimableReturnsTrueWhenQueuedAndAvailable()
    {
        var now = new DateTimeOffset(2024, 06, 01, 12, 0, 0, TimeSpan.Zero);
        var availableAt = now.AddMinutes(-1);

        var result = WorkQueuePolicy.IsClaimable("queued", availableAt, now);

        Assert.True(result);
    }

    [Fact]
    public void IsClaimableReturnsFalseWhenNotQueued()
    {
        var now = new DateTimeOffset(2024, 06, 01, 12, 0, 0, TimeSpan.Zero);
        var availableAt = now.AddMinutes(-1);

        var result = WorkQueuePolicy.IsClaimable("failed", availableAt, now);

        Assert.False(result);
    }

    [Fact]
    public void CalculateNextAvailableCapsAtSixtyMinutes()
    {
        var now = new DateTimeOffset(2024, 06, 01, 12, 0, 0, TimeSpan.Zero);
        var next = WorkQueuePolicy.CalculateNextAvailable(now, attempts: 10);

        Assert.Equal(now.AddMinutes(60), next);
    }
}
