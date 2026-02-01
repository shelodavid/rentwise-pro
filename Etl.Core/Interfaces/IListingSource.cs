using RentWisePro.Etl.Core.Models;

namespace RentWisePro.Etl.Core.Interfaces;

public interface IListingSource
{
    string Name { get; }
    Task<IReadOnlyList<SourceListing>> FetchListingsAsync(SourceFetchRequest request, CancellationToken cancellationToken);
}
