using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Core.Services;

namespace RentWisePro.Etl.Sources.Sources;

public class DevFixtureListingSource : IListingSource
{
    private readonly string _sourceName;
    private readonly string _fixtureRootPath;
    private readonly string _fixtureScenario;
    private readonly AddressNormalizer _addressNormalizer;
    private readonly HashingService _hashingService;
    private readonly ILogger<DevFixtureListingSource> _logger;
    private readonly JsonSerializerOptions _serializerOptions;

    public DevFixtureListingSource(
        string sourceName,
        string fixtureRootPath,
        string? fixtureScenario,
        AddressNormalizer addressNormalizer,
        HashingService hashingService,
        ILogger<DevFixtureListingSource> logger)
    {
        _sourceName = sourceName;
        _fixtureRootPath = fixtureRootPath;
        _fixtureScenario = string.IsNullOrWhiteSpace(fixtureScenario) ? "baseline" : fixtureScenario.Trim();
        _addressNormalizer = addressNormalizer;
        _hashingService = hashingService;
        _logger = logger;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public string Name => _sourceName;

    public async Task<IReadOnlyList<SourceListing>> FetchListingsAsync(SourceFetchRequest request, CancellationToken cancellationToken)
    {
        var listings = await LoadFixtureListingsAsync(cancellationToken);
        if (listings.Count == 0)
        {
            return Array.Empty<SourceListing>();
        }

        var since = request.Since;
        var filtered = listings
            .Where(listing => since is null || listing.UpdatedAt is null || listing.UpdatedAt > since)
            .OrderBy(listing => listing.ListingId ?? listing.SourceListingId ?? listing.Address?.Line ?? string.Empty)
            .ToList();

        var pageSize = request.PageSize > 0 ? request.PageSize : 50;
        var page = request.Page > 0 ? request.Page : 1;
        var skip = (page - 1) * pageSize;
        return filtered
            .Skip(skip)
            .Take(pageSize)
            .Select(MapToSourceListing)
            .ToList();
    }

    private async Task<IReadOnlyList<FixtureListing>> LoadFixtureListingsAsync(CancellationToken cancellationToken)
    {
        var sourceFolder = Path.Combine(_fixtureRootPath, NormalizeFolderName(_sourceName));
        var scenarioFolder = ResolveScenarioFolder(sourceFolder);
        var searchResultsPath = Path.Combine(scenarioFolder, "search-results.json");
        if (!File.Exists(searchResultsPath))
        {
            _logger.LogWarning(
                "Fixture search results not found for source {Source} at {Path}",
                _sourceName,
                searchResultsPath);
            return Array.Empty<FixtureListing>();
        }

        await using var stream = File.OpenRead(searchResultsPath);
        var listings = await JsonSerializer.DeserializeAsync<List<FixtureListing>>(stream, _serializerOptions, cancellationToken)
                      ?? new List<FixtureListing>();

        if (listings.Count == 0)
        {
            return listings;
        }

        var detailsFolder = Path.Combine(scenarioFolder, "listing-details");
        if (!Directory.Exists(detailsFolder))
        {
            return listings;
        }

        for (var index = 0; index < listings.Count; index++)
        {
            var listing = listings[index];
            var listingId = GetListingIdentifier(listing);
            if (string.IsNullOrWhiteSpace(listingId))
            {
                continue;
            }

            var detailPath = Path.Combine(detailsFolder, $"{listingId}.json");
            if (!File.Exists(detailPath))
            {
                continue;
            }

            await using var detailStream = File.OpenRead(detailPath);
            var detail = await JsonSerializer.DeserializeAsync<FixtureListing>(detailStream, _serializerOptions, cancellationToken);
            if (detail is null)
            {
                continue;
            }

            listings[index] = MergeListing(listing, detail);
        }

        return listings;
    }

    private SourceListing MapToSourceListing(FixtureListing fixture)
    {
        var addressLine = CleanValue(fixture.Address?.Line) ?? CleanValue(fixture.AddressLine);
        var city = CleanValue(fixture.Address?.City) ?? CleanValue(fixture.City);
        var state = NormalizeState(CleanValue(fixture.Address?.State) ?? CleanValue(fixture.State));
        var zip = ResolveZip(fixture);

        var normalizedAddress = _addressNormalizer.Normalize(addressLine);
        var sourceListingId = GetListingIdentifier(fixture);
        if (string.IsNullOrWhiteSpace(sourceListingId))
        {
            sourceListingId = _hashingService.ComputeSha256($"{_sourceName}|{normalizedAddress}|{city}|{state}|{zip}");
        }

        return new SourceListing
        {
            SourceListingId = sourceListingId,
            Address = normalizedAddress,
            City = city,
            State = state,
            Zip = zip,
            Latitude = fixture.Latitude,
            Longitude = fixture.Longitude,
            Price = fixture.Price,
            Beds = fixture.Beds,
            Baths = fixture.Baths,
            SquareFeet = fixture.SquareFeet,
            Status = CleanValue(fixture.Status) ?? "active",
            PropertyType = CleanValue(fixture.PropertyType),
            YearBuilt = fixture.YearBuilt,
            LotSize = fixture.LotSize,
            PhotoUrls = fixture.Photos?.Where(url => !string.IsNullOrWhiteSpace(url)).ToList() ?? new List<string>(),
            RawJson = JsonSerializer.Serialize(fixture, _serializerOptions)
        };
    }

    private static string? GetListingIdentifier(FixtureListing listing)
    {
        if (!string.IsNullOrWhiteSpace(listing.SourceListingId))
        {
            return listing.SourceListingId;
        }

        if (!string.IsNullOrWhiteSpace(listing.ListingId))
        {
            return listing.ListingId;
        }

        return null;
    }

    private static FixtureListing MergeListing(FixtureListing baseListing, FixtureListing detail)
    {
        return new FixtureListing
        {
            ListingId = detail.ListingId ?? baseListing.ListingId,
            SourceListingId = detail.SourceListingId ?? baseListing.SourceListingId,
            Address = detail.Address ?? baseListing.Address,
            AddressLine = detail.AddressLine ?? baseListing.AddressLine,
            City = detail.City ?? baseListing.City,
            State = detail.State ?? baseListing.State,
            PostalCode = detail.PostalCode ?? baseListing.PostalCode,
            PostalCodeCamel = detail.PostalCodeCamel ?? baseListing.PostalCodeCamel,
            Zip = detail.Zip ?? baseListing.Zip,
            ZipCode = detail.ZipCode ?? baseListing.ZipCode,
            Latitude = detail.Latitude ?? baseListing.Latitude,
            Longitude = detail.Longitude ?? baseListing.Longitude,
            Price = detail.Price ?? baseListing.Price,
            Beds = detail.Beds ?? baseListing.Beds,
            Baths = detail.Baths ?? baseListing.Baths,
            SquareFeet = detail.SquareFeet ?? baseListing.SquareFeet,
            Status = detail.Status ?? baseListing.Status,
            PropertyType = detail.PropertyType ?? baseListing.PropertyType,
            YearBuilt = detail.YearBuilt ?? baseListing.YearBuilt,
            LotSize = detail.LotSize ?? baseListing.LotSize,
            Photos = detail.Photos?.Count > 0 ? detail.Photos : baseListing.Photos,
            UpdatedAt = detail.UpdatedAt ?? baseListing.UpdatedAt
        };
    }

    private static string NormalizeFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "fixtures";
        }

        var normalized = name.Trim().ToLowerInvariant();
        Span<char> buffer = stackalloc char[normalized.Length];
        var index = 0;
        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer[index++] = ch;
            }
            else if (ch == ' ' || ch == '_' || ch == '-')
            {
                buffer[index++] = '-';
            }
        }

        var result = new string(buffer[..index]);
        return result.Trim('-');
    }

    private string ResolveScenarioFolder(string sourceFolder)
    {
        var scenarioFolder = Path.Combine(sourceFolder, NormalizeFolderName(_fixtureScenario));
        if (Directory.Exists(scenarioFolder))
        {
            return scenarioFolder;
        }

        return sourceFolder;
    }

    private static string? CleanValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? ResolveZip(FixtureListing fixture)
    {
        return FirstNonEmpty(
            fixture.Address?.PostalCode,
            fixture.Address?.PostalCodeCamel,
            fixture.Address?.Zip,
            fixture.Address?.ZipCode,
            fixture.PostalCode,
            fixture.PostalCodeCamel,
            fixture.Zip,
            fixture.ZipCode);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            var cleaned = CleanValue(value);
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                return cleaned;
            }
        }

        return null;
    }

    private static string? NormalizeState(string? state)
    {
        return string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToUpperInvariant();
    }

    private sealed class FixtureListing
    {
        [JsonPropertyName("listing_id")]
        public string? ListingId { get; init; }

        [JsonPropertyName("source_listing_id")]
        public string? SourceListingId { get; init; }

        [JsonPropertyName("address")]
        public FixtureAddress? Address { get; init; }

        [JsonPropertyName("address_line")]
        public string? AddressLine { get; init; }

        [JsonPropertyName("city")]
        public string? City { get; init; }

        [JsonPropertyName("state")]
        public string? State { get; init; }

        [JsonPropertyName("postal_code")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? PostalCode { get; init; }

        [JsonPropertyName("postalCode")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? PostalCodeCamel { get; init; }

        [JsonPropertyName("zip")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? Zip { get; init; }

        [JsonPropertyName("zipcode")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? ZipCode { get; init; }

        [JsonPropertyName("latitude")]
        public decimal? Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public decimal? Longitude { get; init; }

        [JsonPropertyName("price")]
        public decimal? Price { get; init; }

        [JsonPropertyName("beds")]
        public decimal? Beds { get; init; }

        [JsonPropertyName("baths")]
        public decimal? Baths { get; init; }

        [JsonPropertyName("sqft")]
        public int? SquareFeet { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("property_type")]
        public string? PropertyType { get; init; }

        [JsonPropertyName("year_built")]
        public int? YearBuilt { get; init; }

        [JsonPropertyName("lot_size")]
        public decimal? LotSize { get; init; }

        [JsonPropertyName("photos")]
        public List<string>? Photos { get; init; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset? UpdatedAt { get; init; }
    }

    private sealed class FixtureAddress
    {
        [JsonPropertyName("line")]
        public string? Line { get; init; }

        [JsonPropertyName("city")]
        public string? City { get; init; }

        [JsonPropertyName("state")]
        public string? State { get; init; }

        [JsonPropertyName("postal_code")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? PostalCode { get; init; }

        [JsonPropertyName("postalCode")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? PostalCodeCamel { get; init; }

        [JsonPropertyName("zip")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? Zip { get; init; }

        [JsonPropertyName("zipcode")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? ZipCode { get; init; }
    }

    private sealed class FlexibleStringConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetRawText(),
                JsonTokenType.Null => null,
                _ => throw new JsonException($"Unsupported token type {reader.TokenType} for flexible string.")
            };
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value);
        }
    }
}
