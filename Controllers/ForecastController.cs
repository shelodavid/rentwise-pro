using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models.Forecast;
using RentWisePro.Web.Services;

namespace RentWisePro.Web.Controllers
{
    public class ForecastController : Controller
    {
        private readonly RentWiseProDbContext _dbContext;
        private readonly ForecastCalculationService _forecastCalculationService;

        public ForecastController(RentWiseProDbContext dbContext, ForecastCalculationService forecastCalculationService)
        {
            _dbContext = dbContext;
            _forecastCalculationService = forecastCalculationService;
        }

        [HttpGet("/Home/Forecast")]
        [HttpGet("/Forecast/{zpid:long?}")]
        public async Task<IActionResult> Forecast(long? zpid)
        {
            if (!zpid.HasValue)
            {
                return BadRequest("A zpid query parameter is required.");
            }

            var listing = await _dbContext.RentalListings.AsNoTracking()
                .FirstOrDefaultAsync(listing => listing.Zpid == zpid.Value);

            if (listing is null)
            {
                return NotFound("Listing not found.");
            }

            var investmentProfile = await _dbContext.InvestmentProfiles.AsNoTracking()
                .FirstOrDefaultAsync(profile => profile.Id == 1);

            if (investmentProfile is null)
            {
                return NotFound("Default investment profile not found.");
            }

            var savedProfile = await _dbContext.SavedPropertyProfiles.AsNoTracking()
                .FirstOrDefaultAsync(profile => profile.RentalListingId == listing.RentalListingId
                                                && profile.InvestmentProfileId == investmentProfile.Id);

            var calculation = _forecastCalculationService.Calculate(listing, investmentProfile, savedProfile);

            var addressLine = listing.StreetAddress ?? "Unknown address";
            var locationLine = string.Join(", ", new[]
            {
                listing.City,
                listing.State,
                listing.ZipCode
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

            var viewModel = new ForecastPageVm
            {
                Listing = new ListingSummary
                {
                    Zpid = listing.Zpid,
                    AddressLine = addressLine,
                    LocationLine = locationLine,
                    PropertyType = listing.PropertyType,
                    ImgSrc = listing.ImgSrc,
                    Price = listing.Price ?? 0m,
                    EstimatedRent = listing.EstimatedRent ?? 0m,
                    Bedrooms = listing.Bedrooms,
                    Bathrooms = listing.Bathrooms
                },
                Assumptions = calculation.Assumptions,
                Kpis = calculation.Kpis
            };

            return View("~/Views/Home/Forecast.cshtml", viewModel);
        }
    }
}
