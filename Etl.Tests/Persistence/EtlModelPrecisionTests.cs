using Microsoft.EntityFrameworkCore;
using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Persistence.Contexts;
using Xunit;

namespace RentWisePro.Etl.Tests.Persistence;

public class EtlModelPrecisionTests
{
    [Fact]
    public void ModelDefinesExpectedDecimalPrecision()
    {
        var options = new DbContextOptionsBuilder<EtlDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=RentWisePro.Tests;Trusted_Connection=True;")
            .Options;

        using var context = new EtlDbContext(options);
        var model = context.Model;

        var listingPrice = model.FindEntityType(typeof(Listing))!
            .FindProperty(nameof(Listing.Price))!;
        Assert.Equal("decimal(18,0)", listingPrice.GetColumnType());

        var snapshotPrice = model.FindEntityType(typeof(ListingSnapshot))!
            .FindProperty(nameof(ListingSnapshot.Price))!;
        Assert.Equal("decimal(18,0)", snapshotPrice.GetColumnType());

        var propertyEntity = model.FindEntityType(typeof(Property))!;
        var beds = propertyEntity.FindProperty(nameof(Property.Beds))!;
        Assert.Equal(4, beds.GetPrecision());
        Assert.Equal(1, beds.GetScale());

        var baths = propertyEntity.FindProperty(nameof(Property.Baths))!;
        Assert.Equal(4, baths.GetPrecision());
        Assert.Equal(1, baths.GetScale());

        var latitude = propertyEntity.FindProperty(nameof(Property.Latitude))!;
        Assert.Equal(9, latitude.GetPrecision());
        Assert.Equal(6, latitude.GetScale());

        var longitude = propertyEntity.FindProperty(nameof(Property.Longitude))!;
        Assert.Equal(9, longitude.GetPrecision());
        Assert.Equal(6, longitude.GetScale());
    }
}
