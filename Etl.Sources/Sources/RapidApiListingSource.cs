using System.Text.Json;
using Microsoft.Extensions.Logging;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Core.Options;
using RentWisePro.Etl.Sources.Clients;
using RentWisePro.Etl.Sources.RateLimiting;

namespace RentWisePro.Etl.Sources.Sources;

public class RapidApiListingSource : IListingSource
{
    private readonly RapidApiClient _client;
    private readonly RapidApiSourceOptions _options;
    private readonly string _apiKey;
    private readonly ILogger<RapidApiListingSource> _logger;
    private readonly SourceRateLimiter _rateLimiter;

    public RapidApiListingSource(
        RapidApiClient client,
        RapidApiSourceOptions options,
        string apiKey,
        ILogger<RapidApiListingSource> logger)
    {
        _client = client;
        _options = options;
        _apiKey = apiKey;
        _logger = logger;
        _rateLimiter = new SourceRateLimiter(options.MaxConcurrency, options.MaxRequestsPerMinute);
    }

    public string Name => _options.Name;

    public async Task<IReadOnlyList<SourceListing>> FetchListingsAsync(SourceFetchRequest request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : _options.PageSize;
        var endpoint = _options.EndpointTemplate
            .Replace("{page}", request.Page.ToString())
            .Replace("{pageSize}", pageSize.ToString())
            .Replace("{since}", request.Since?.ToString("O") ?? string.Empty);

        var uri = new Uri(new Uri(_options.BaseUrl), endpoint);

        using var limiter = await _rateLimiter.WaitAsync(cancellationToken);
        var json = await _client.GetJsonAsync(uri, _apiKey, _options.Host, cancellationToken);
        if (json is null)
        {
            return Array.Empty<SourceListing>();
        }

        return ParseListings(json);
    }

    private IReadOnlyList<SourceListing> ParseListings(JsonDocument json)
    {
        var listingsElement = FindListingsArray(json.RootElement);
        if (listingsElement.ValueKind != JsonValueKind.Array)
        {
            _logger.LogWarning("RapidAPI response did not contain listing array for {Source}", _options.Name);
            return Array.Empty<SourceListing>();
        }

        var listings = new List<SourceListing>();
        foreach (var item in listingsElement.EnumerateArray())
        {
            var listing = new SourceListing
            {
                SourceListingId = GetString(item, "id") ??
                                  GetString(item, "listingId") ??
                                  GetString(item, "property_id") ??
                                  GetString(item, "propertyId") ??
                                  Guid.NewGuid().ToString("N"),
                Address = GetNestedString(item, "address", "line") ?? GetString(item, "address") ?? GetString(item, "street") ?? GetString(item, "streetAddress"),
                City = GetNestedString(item, "address", "city") ?? GetString(item, "city"),
                State = GetNestedString(item, "address", "state") ?? GetString(item, "state"),
                Zip = GetNestedString(item, "address", "postal_code") ?? GetString(item, "zip") ?? GetString(item, "postalCode"),
                Latitude = GetDecimal(item, "lat") ?? GetDecimal(item, "latitude"),
                Longitude = GetDecimal(item, "lng") ?? GetDecimal(item, "longitude"),
                Price = GetDecimal(item, "price") ?? GetDecimal(item, "listPrice"),
                MonthlyRent = GetDecimal(item, "rent") ?? GetDecimal(item, "monthlyRent") ?? GetDecimal(item, "estimatedRent"),
                Beds = GetDecimal(item, "beds") ?? GetDecimal(item, "bedrooms"),
                Baths = GetDecimal(item, "baths") ?? GetDecimal(item, "bathrooms"),
                SquareFeet = GetInt(item, "sqft") ?? GetInt(item, "livingArea"),
                Status = GetString(item, "status") ?? GetString(item, "listingStatus") ?? "active",
                PropertyType = GetString(item, "propertyType") ?? GetString(item, "type"),
                YearBuilt = GetInt(item, "yearBuilt")
            };

            listing.PhotoUrls = GetPhotoUrls(item).Take(10).ToList();
            listing.RawJson = item.GetRawText();
            listings.Add(listing);
        }

        return listings;
    }

    private static JsonElement FindListingsArray(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root;
        }

        if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
        {
            return dataElement;
        }

        if (root.TryGetProperty("listings", out var listings) && listings.ValueKind == JsonValueKind.Array)
        {
            return listings;
        }

        if (root.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            return results;
        }

        return default;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value))
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
        }

        return null;
    }

    private static string? GetNestedString(JsonElement element, string container, string propertyName)
    {
        if (element.TryGetProperty(container, out var child) && child.ValueKind == JsonValueKind.Object)
        {
            return GetString(child, propertyName);
        }

        return null;
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var result))
        {
            return result;
        }

        if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static int? GetInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result))
        {
            return result;
        }

        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static IEnumerable<string> GetPhotoUrls(JsonElement element)
    {
        if (element.TryGetProperty("photos", out var photos) && photos.ValueKind == JsonValueKind.Array)
        {
            foreach (var photo in photos.EnumerateArray())
            {
                if (photo.ValueKind == JsonValueKind.String)
                {
                    var url = photo.GetString();
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        yield return url;
                    }
                }
                else if (photo.ValueKind == JsonValueKind.Object)
                {
                    var url = GetString(photo, "url") ?? GetString(photo, "href");
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        yield return url;
                    }
                }
            }
        }

        if (element.TryGetProperty("primary_photo", out var primary) && primary.ValueKind == JsonValueKind.Object)
        {
            var url = GetString(primary, "href") ?? GetString(primary, "url");
            if (!string.IsNullOrWhiteSpace(url))
            {
                yield return url;
            }
        }
    }
}
