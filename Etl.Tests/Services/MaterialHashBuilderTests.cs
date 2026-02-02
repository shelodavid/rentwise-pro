using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Core.Services;
using Xunit;

namespace RentWisePro.Etl.Tests.Services;

public class MaterialHashBuilderTests
{
    [Fact]
    public void Build_WhenListingUnchanged_ReturnsSameHash()
    {
        var hashingService = new HashingService();
        var builder = new MaterialHashBuilder(hashingService);

        var listing = new SourceListing
        {
            Address = "123 Main St",
            City = "Austin",
            State = "TX",
            Zip = "78701",
            Price = 250000m,
            Beds = 3,
            Baths = 2
        };

        var hash1 = builder.Build(listing);
        var hash2 = builder.Build(listing);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Build_WhenPriceChanges_ReturnsDifferentHash()
    {
        var hashingService = new HashingService();
        var builder = new MaterialHashBuilder(hashingService);

        var listing = new SourceListing
        {
            Address = "123 Main St",
            City = "Austin",
            State = "TX",
            Zip = "78701",
            Price = 250000m,
            Beds = 3,
            Baths = 2
        };

        var hash1 = builder.Build(listing);
        listing.Price = 260000m;
        var hash2 = builder.Build(listing);

        Assert.NotEqual(hash1, hash2);
    }
}
