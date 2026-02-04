using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Core.Services;
using RentWisePro.Etl.Sources.Sources;
using Xunit;

namespace RentWisePro.Etl.Tests.Sources;

public class DevFixtureListingSourceTests
{
    [Fact]
    public async Task FetchListingsAsync_LoadsAndMapsFixtureListings()
    {
        var fixtureRoot = CreateFixtureRoot();
        await WriteFixtureAsync(fixtureRoot, "fixture-source", new[]
        {
            new
            {
                listing_id = "TEST-1001",
                address_line = "123 Main St",
                city = "Austin",
                state = "tx",
                postal_code = "78701",
                price = 300000,
                beds = 2,
                baths = 1.5m,
                sqft = 900,
                status = "active",
                property_type = "condo",
                year_built = 2001,
                photos = new[] { "https://example.com/photo-1.jpg" },
                updated_at = "2024-05-01T00:00:00Z"
            }
        });

        var source = BuildSource(fixtureRoot, "fixture-source");

        var listings = await source.FetchListingsAsync(new SourceFetchRequest(1, 10, null), CancellationToken.None);

        var listing = Assert.Single(listings);
        Assert.Equal("TEST-1001", listing.SourceListingId);
        Assert.Equal("123 MAIN ST", listing.Address);
        Assert.Equal("Austin", listing.City);
        Assert.Equal("TX", listing.State);
        Assert.Equal("78701", listing.Zip);
        Assert.Equal(300000, listing.Price);
        Assert.Equal(2, listing.Beds);
        Assert.Equal(1.5m, listing.Baths);
        Assert.Equal(900, listing.SquareFeet);
        Assert.Equal("active", listing.Status);
        Assert.Equal("condo", listing.PropertyType);
        Assert.Equal(2001, listing.YearBuilt);
        Assert.Single(listing.PhotoUrls);
        Assert.Contains("\"listing_id\":\"TEST-1001\"", listing.RawJson);
    }

    [Fact]
    public async Task FetchListingsAsync_FiltersBySince()
    {
        var fixtureRoot = CreateFixtureRoot();
        await WriteFixtureAsync(fixtureRoot, "fixture-source", new[]
        {
            new
            {
                listing_id = "TEST-1001",
                address_line = "123 Main St",
                city = "Austin",
                state = "TX",
                postal_code = "78701",
                updated_at = "2024-05-01T00:00:00Z"
            },
            new
            {
                listing_id = "TEST-1002",
                address_line = "124 Main St",
                city = "Austin",
                state = "TX",
                postal_code = "78701",
                updated_at = "2024-05-03T00:00:00Z"
            }
        });

        var source = BuildSource(fixtureRoot, "fixture-source");
        var since = DateTimeOffset.Parse("2024-05-02T00:00:00Z");

        var listings = await source.FetchListingsAsync(new SourceFetchRequest(1, 10, since), CancellationToken.None);

        var listing = Assert.Single(listings);
        Assert.Equal("TEST-1002", listing.SourceListingId);
    }

    [Fact]
    public async Task FetchListingsAsync_GeneratesStableIdsAndNormalizesAddresses()
    {
        var fixtureRoot = CreateFixtureRoot();
        await WriteFixtureAsync(fixtureRoot, "fixture-source", new[]
        {
            new
            {
                address_line = "  500   West 6th St ",
                city = "Austin",
                state = "tx",
                postal_code = "78701",
                updated_at = "2024-05-01T00:00:00Z"
            }
        });

        var source = BuildSource(fixtureRoot, "fixture-source");

        var firstRun = await source.FetchListingsAsync(new SourceFetchRequest(1, 10, null), CancellationToken.None);
        var secondRun = await source.FetchListingsAsync(new SourceFetchRequest(1, 10, null), CancellationToken.None);

        var firstListing = Assert.Single(firstRun);
        var secondListing = Assert.Single(secondRun);
        Assert.Equal(firstListing.SourceListingId, secondListing.SourceListingId);
        Assert.Equal("500 WEST 6TH ST", firstListing.Address);
    }

    [Fact]
    public async Task FetchListingsAsync_ReturnsDeterministicPages()
    {
        var fixtureRoot = CreateFixtureRoot();
        var listings = Enumerable.Range(1, 12).Select(index => new
        {
            listing_id = $"LIST-{index:000}",
            address_line = $"{index} Main St",
            city = "Austin",
            state = "TX",
            postal_code = "78701",
            updated_at = "2024-05-01T00:00:00Z"
        });
        await WriteFixtureAsync(fixtureRoot, "fixture-source", listings);

        var source = BuildSource(fixtureRoot, "fixture-source");

        var page2 = await source.FetchListingsAsync(new SourceFetchRequest(2, 5, null), CancellationToken.None);

        Assert.Equal(5, page2.Count);
        Assert.Equal("LIST-006", page2[0].SourceListingId);
        Assert.Equal("LIST-010", page2[^1].SourceListingId);
    }

    private static DevFixtureListingSource BuildSource(string fixtureRoot, string sourceName, string? scenario = null)
    {
        return new DevFixtureListingSource(
            sourceName,
            fixtureRoot,
            scenario,
            new AddressNormalizer(),
            new HashingService(),
            NullLogger<DevFixtureListingSource>.Instance);
    }

    private static string CreateFixtureRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "rentwise-fixtures", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static async Task WriteFixtureAsync(string fixtureRoot, string sourceName, IEnumerable<object> listings)
    {
        var sourceFolder = Path.Combine(fixtureRoot, sourceName);
        Directory.CreateDirectory(sourceFolder);
        var path = Path.Combine(sourceFolder, "search-results.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(listings));
    }
}
