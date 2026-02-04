using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models;

namespace RentWisePro.Web.Controllers;

[Authorize]
public class ListingsController : Controller
{
    private static readonly string[] DefaultStatusOptions =
    {
        "active",
        "pending",
        "sold",
        "removed"
    };

    private readonly RentWiseProDbContext _dbContext;

    public ListingsController(RentWiseProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? status,
        string? source,
        string? city,
        string? state,
        string? sortBy)
    {
        var listingsQuery = _dbContext.EtlListings
            .AsNoTracking()
            .Include(listing => listing.Property);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusTerm = status.Trim();
            listingsQuery = listingsQuery.Where(listing => listing.Status == statusTerm);
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            var sourceTerm = source.Trim();
            listingsQuery = listingsQuery.Where(listing => listing.Source == sourceTerm);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var cityTerm = city.Trim();
            listingsQuery = listingsQuery.Where(listing =>
                listing.Property != null
                && listing.Property.City != null
                && listing.Property.City.Contains(cityTerm));
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            var stateTerm = state.Trim();
            listingsQuery = listingsQuery.Where(listing =>
                listing.Property != null
                && listing.Property.State != null
                && listing.Property.State.Contains(stateTerm));
        }

        listingsQuery = string.Equals(sortBy, "price", StringComparison.OrdinalIgnoreCase)
            ? listingsQuery.OrderByDescending(listing => listing.Price)
                .ThenByDescending(listing => listing.LastSeenAt)
            : listingsQuery.OrderByDescending(listing => listing.LastSeenAt)
                .ThenByDescending(listing => listing.Price);

        var listings = await listingsQuery.ToListAsync();
        var propertyIds = listings.Select(listing => listing.PropertyId).Distinct().ToList();

        var photos = propertyIds.Count == 0
            ? new List<PropertyPhotoLookup>()
            : await _dbContext.EtlPropertyPhotos
                .AsNoTracking()
                .Where(photo => propertyIds.Contains(photo.PropertyId))
                .OrderBy(photo => photo.PhotoIndex)
                .Select(photo => new PropertyPhotoLookup
                {
                    PropertyId = photo.PropertyId,
                    Url = !string.IsNullOrWhiteSpace(photo.UrlOriginal)
                        ? photo.UrlOriginal
                        : photo.StoragePath
                })
                .ToListAsync();

        var photoLookup = photos
            .Where(photo => !string.IsNullOrWhiteSpace(photo.Url))
            .GroupBy(photo => photo.PropertyId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(photo => photo.Url!)
                    .Take(10)
                    .ToList());

        var listingRows = listings.Select(listing =>
        {
            photoLookup.TryGetValue(listing.PropertyId, out var listingPhotos);

            return new EtlListingRowVm
            {
                ListingId = listing.ListingId,
                Street = listing.Property?.Street,
                City = listing.Property?.City,
                State = listing.Property?.State,
                Zip = listing.Property?.Zip,
                Price = listing.Price,
                Status = listing.Status,
                Beds = listing.Property?.Beds,
                Baths = listing.Property?.Baths,
                SquareFeet = listing.Property?.SquareFeet,
                Source = listing.Source,
                SourceListingId = listing.SourceListingId,
                LastSeenAt = listing.LastSeenAt,
                PhotoUrls = listingPhotos ?? new List<string>()
            };
        }).ToList();

        var sources = await _dbContext.EtlListings
            .AsNoTracking()
            .Select(listing => listing.Source)
            .Distinct()
            .OrderBy(listingSource => listingSource)
            .ToListAsync();

        var latestRun = await _dbContext.EtlRuns
            .AsNoTracking()
            .OrderByDescending(run => run.StartedAt)
            .Select(run => new EtlRunVm
            {
                StartedAt = run.StartedAt,
                FinishedAt = run.FinishedAt,
                Status = run.Status
            })
            .FirstOrDefaultAsync();

        var statusCounts = await _dbContext.EtlListings
            .AsNoTracking()
            .GroupBy(listing => listing.Status)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync();

        var viewModel = new EtlListingsIndexVm
        {
            Listings = listingRows,
            Sources = sources
                .Select(listingSource => new SelectListItem(listingSource, listingSource, listingSource == source))
                .ToList(),
            Statuses = DefaultStatusOptions
                .Select(option => new SelectListItem(option, option, option == status))
                .ToList(),
            StatusOptions = DefaultStatusOptions.ToList(),
            Status = status,
            Source = source,
            City = city,
            State = state,
            SortBy = sortBy,
            Health = new EtlHealthVm
            {
                LatestRun = latestRun,
                ListingCountsByStatus = statusCounts.ToDictionary(item => item.Key, item => item.Count, StringComparer.OrdinalIgnoreCase)
            }
        };

        return View(viewModel);
    }

    private sealed class PropertyPhotoLookup
    {
        public Guid PropertyId { get; set; }
        public string? Url { get; set; }
    }
}
