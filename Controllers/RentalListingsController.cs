using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models;

namespace RentWisePro.Web.Controllers
{
    public class RentalListingsController : Controller
    {
        private const int DefaultPageSize = 24;
        private const int MaxPageSize = 96;
        private readonly RentWiseProDbContext _dbContext;

        public RentalListingsController(RentWiseProDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? city,
            string? state,
            decimal? minPrice,
            decimal? maxPrice,
            int? minBedrooms,
            decimal? minBathrooms,
            string? propertyType,
            int page = 1,
            int? pageSize = null)
        {
            var listingsQuery = _dbContext.RentalListings.AsNoTracking();
            var normalizedPageSize = NormalizePageSize(pageSize);
            var normalizedMinPrice = NormalizeNonNegative(minPrice);
            var normalizedMaxPrice = NormalizeNonNegative(maxPrice);
            var normalizedMinBedrooms = NormalizeNonNegative(minBedrooms);
            var normalizedMinBathrooms = NormalizeNonNegative(minBathrooms);

            if (normalizedMinPrice.HasValue && normalizedMaxPrice.HasValue
                && normalizedMinPrice.Value > normalizedMaxPrice.Value)
            {
                (normalizedMinPrice, normalizedMaxPrice) = (normalizedMaxPrice, normalizedMinPrice);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                listingsQuery = listingsQuery.Where(listing =>
                    (listing.StreetAddress != null && listing.StreetAddress.Contains(term)) ||
                    (listing.City != null && listing.City.Contains(term)) ||
                    (listing.State != null && listing.State.Contains(term)) ||
                    (listing.ZipCode != null && listing.ZipCode.Contains(term)) ||
                    (listing.PropertyType != null && listing.PropertyType.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityTerm = city.Trim();
                listingsQuery = listingsQuery.Where(listing =>
                    listing.City != null && listing.City.Contains(cityTerm));
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                var stateTerm = state.Trim().ToUpperInvariant();
                listingsQuery = listingsQuery.Where(listing =>
                    listing.State != null && listing.State.ToUpper() == stateTerm);
            }

            if (!string.IsNullOrWhiteSpace(propertyType))
            {
                var typeTerm = propertyType.Trim();
                listingsQuery = listingsQuery.Where(listing =>
                    listing.PropertyType != null && listing.PropertyType == typeTerm);
            }

            if (normalizedMinPrice.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Price >= normalizedMinPrice);
            }

            if (normalizedMaxPrice.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Price <= normalizedMaxPrice);
            }

            if (normalizedMinBedrooms.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Bedrooms >= normalizedMinBedrooms);
            }

            if (normalizedMinBathrooms.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Bathrooms >= normalizedMinBathrooms);
            }

            listingsQuery = listingsQuery
                .OrderByDescending(listing => listing.IngestedAtUtc)
                .ThenByDescending(listing => listing.Price)
                .ThenBy(listing => listing.RentalListingId);

            var totalCount = await listingsQuery.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)normalizedPageSize));
            var normalizedPage = Math.Clamp(page, 1, totalPages);

            var listings = await listingsQuery
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(listing => new RentalListingCardVm
                {
                    RentalListingId = listing.RentalListingId,
                    Zpid = listing.Zpid,
                    StreetAddress = listing.StreetAddress,
                    City = listing.City,
                    State = listing.State,
                    ZipCode = listing.ZipCode,
                    PropertyType = listing.PropertyType,
                    Price = listing.Price,
                    Bedrooms = listing.Bedrooms,
                    Bathrooms = listing.Bathrooms,
                    ImgSrc = listing.ImgSrc,
                    IngestedAtUtc = listing.IngestedAtUtc
                })
                .ToListAsync();

            var propertyTypes = await _dbContext.RentalListings
                .AsNoTracking()
                .Where(listing => listing.PropertyType != null && listing.PropertyType != string.Empty)
                .Select(listing => listing.PropertyType!)
                .Distinct()
                .OrderBy(type => type)
                .ToListAsync();

            var viewModel = new RentalListingsIndexVm
            {
                Listings = listings,
                PropertyTypes = propertyTypes.Select(type => new SelectListItem(type, type)).ToList(),
                Search = search,
                City = city,
                State = state,
                MinPrice = normalizedMinPrice,
                MaxPrice = normalizedMaxPrice,
                MinBedrooms = normalizedMinBedrooms,
                MinBathrooms = normalizedMinBathrooms,
                PropertyType = propertyType,
                Page = normalizedPage,
                PageSize = normalizedPageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        private static int NormalizePageSize(int? pageSize)
        {
            if (!pageSize.HasValue || pageSize <= 0)
            {
                return DefaultPageSize;
            }

            return Math.Min(pageSize.Value, MaxPageSize);
        }

        private static decimal? NormalizeNonNegative(decimal? value)
        {
            return value.HasValue && value.Value >= 0 ? value : null;
        }

        private static int? NormalizeNonNegative(int? value)
        {
            return value.HasValue && value.Value >= 0 ? value : null;
        }
    }
}
