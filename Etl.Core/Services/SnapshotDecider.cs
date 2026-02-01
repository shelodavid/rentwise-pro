namespace RentWisePro.Etl.Core.Services;

public class SnapshotDecider
{
    public bool ShouldCreateSnapshot(string? previousHash, string currentHash)
    {
        return !string.Equals(previousHash, currentHash, StringComparison.OrdinalIgnoreCase);
    }
}
