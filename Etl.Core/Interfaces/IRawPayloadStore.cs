namespace RentWisePro.Etl.Core.Interfaces;

public interface IRawPayloadStore
{
    Task<string> SaveAsync(string source, string sourceListingId, string rawJson, CancellationToken cancellationToken);
}
