using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models.Forecast;
using RentWisePro.Web.Services;

namespace RentWisePro.Web.Controllers
{
    [Authorize]
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
        [HttpGet("/Forecast/{savedPropertyProfileId:int?}")]
        public async Task<IActionResult> Forecast(int? savedPropertyProfileId)
        {
            if (!savedPropertyProfileId.HasValue || savedPropertyProfileId.Value <= 0)
            {
                return RedirectToListingsWithMessage();
            }

            var userId = CurrentUserId;
            var savedProfile = await _dbContext.SavedPropertyProfiles.AsNoTracking()
                .FirstOrDefaultAsync(profile => profile.SavedPropertyProfileId == savedPropertyProfileId.Value
                                                && profile.UserId == userId);

            if (savedProfile is null)
            {
                return RedirectToListingsWithMessage();
            }

            var listing = await _dbContext.RentalListings.AsNoTracking()
                .FirstOrDefaultAsync(listing => listing.RentalListingId == savedProfile.RentalListingId);

            if (listing is null)
            {
                return NotFound("Listing not found.");
            }

            var investmentProfile = await _dbContext.InvestmentProfiles.AsNoTracking()
                .FirstOrDefaultAsync(profile => profile.Id == savedProfile.InvestmentProfileId
                                                && profile.UserId == userId);

            if (investmentProfile is null)
            {
                return NotFound("Default investment profile not found.");
            }

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
                SavedPropertyProfileId = savedProfile.SavedPropertyProfileId,
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

            ViewData["CurrentSavedPropertyProfileId"] = savedProfile.SavedPropertyProfileId;
            return View("~/Views/Home/Forecast.cshtml", viewModel);
        }

        private IActionResult RedirectToListingsWithMessage()
        {
            TempData["StatusMessage"] = "Start an analysis first.";
            return RedirectToAction("Index", "Home");
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
