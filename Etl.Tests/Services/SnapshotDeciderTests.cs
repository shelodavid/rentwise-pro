using RentWisePro.Etl.Core.Services;
using Xunit;

namespace RentWisePro.Etl.Tests.Services;

public class SnapshotDeciderTests
{
    [Fact]
    public void ShouldCreateSnapshot_WhenHashUnchanged_ReturnsFalse()
    {
        var decider = new SnapshotDecider();
        var previous = "abc";
        var current = "abc";

        var shouldCreate = decider.ShouldCreateSnapshot(previous, current);

        Assert.False(shouldCreate);
    }

    [Fact]
    public void ShouldCreateSnapshot_WhenHashChanges_ReturnsTrue()
    {
        var decider = new SnapshotDecider();
        var previous = "abc";
        var current = "def";

        var shouldCreate = decider.ShouldCreateSnapshot(previous, current);

        Assert.True(shouldCreate);
    }
}
