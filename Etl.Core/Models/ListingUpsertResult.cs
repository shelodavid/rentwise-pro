using RentWisePro.Etl.Core.Entities;

namespace RentWisePro.Etl.Core.Models;

public record ListingUpsertResult(Listing Listing, string? PreviousMaterialHash);
