using RentWisePro.Etl.Core.Models;

namespace RentWisePro.Etl.Core.Services;

public class MaterialHashBuilder
{
    private readonly HashingService _hashingService;

    public MaterialHashBuilder(HashingService hashingService)
    {
        _hashingService = hashingService;
    }

    public string Build(SourceListing listing)
    {
        var parts = new[]
        {
            listing.Price?.ToString("0.##") ?? string.Empty,
            listing.Status ?? string.Empty,
            listing.Beds?.ToString("0.##") ?? string.Empty,
            listing.Baths?.ToString("0.##") ?? string.Empty,
            listing.SquareFeet?.ToString() ?? string.Empty,
            listing.Address ?? string.Empty,
            listing.City ?? string.Empty,
            listing.State ?? string.Empty,
            listing.Zip ?? string.Empty,
            listing.LotSize?.ToString("0.##") ?? string.Empty,
            listing.PropertyType ?? string.Empty,
            listing.YearBuilt?.ToString() ?? string.Empty
        };

        var raw = string.Join("|", parts.Select(p => p.Trim().ToUpperInvariant()));
        return _hashingService.ComputeSha256(raw);
    }
}
