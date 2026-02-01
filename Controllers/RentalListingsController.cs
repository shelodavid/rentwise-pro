using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models;

namespace RentWisePro.Web.Controllers
{
    public class RentalListingsController : Controller
    {
        private const int DefaultPageSize = 12;
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
            int page = 1)
        {
            var listingsQuery = _dbContext.RentalListings.AsNoTracking();

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

            if (minPrice.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Price <= maxPrice);
            }

            if (minBedrooms.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Bedrooms >= minBedrooms);
            }

            if (minBathrooms.HasValue)
            {
                listingsQuery = listingsQuery.Where(listing => listing.Bathrooms >= minBathrooms);
            }

            listingsQuery = listingsQuery
                .OrderByDescending(listing => listing.IngestedAtUtc)
                .ThenByDescending(listing => listing.Price)
                .ThenBy(listing => listing.RentalListingId);

            var totalCount = await listingsQuery.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)DefaultPageSize));
            var normalizedPage = Math.Clamp(page, 1, totalPages);

            var listings = await listingsQuery
                .Skip((normalizedPage - 1) * DefaultPageSize)
                .Take(DefaultPageSize)
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
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinBedrooms = minBedrooms,
                MinBathrooms = minBathrooms,
                PropertyType = propertyType,
                Page = normalizedPage,
                PageSize = DefaultPageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            return View(viewModel);
        }
    }
}
