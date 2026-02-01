using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models.SavedAnalyses;

namespace RentWisePro.Web.Controllers
{
    [Authorize]
    [Route("SavedAnalyses")]
    [Authorize]
    public class SavedAnalysesController : Controller
    {
        private readonly RentWiseProDbContext _dbContext;

        public SavedAnalysesController(RentWiseProDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            var analyses = await _dbContext.SavedPropertyProfiles
                .AsNoTracking()
                .Where(profile => profile.UserId == userId)
                .Include(profile => profile.RentalListing)
                .Include(profile => profile.InvestmentProfile)
                .OrderByDescending(profile => profile.SavedAtUtc)
                .Select(profile => new SavedAnalysisSummaryVm
                {
                    SavedPropertyProfileId = profile.SavedPropertyProfileId,
                    StreetAddress = profile.RentalListing.StreetAddress,
                    City = profile.RentalListing.City,
                    State = profile.RentalListing.State,
                    ZipCode = profile.RentalListing.ZipCode,
                    Price = profile.RentalListing.Price,
                    SavedAtUtc = profile.SavedAtUtc,
                    InvestmentProfileName = profile.InvestmentProfile.InvestmentProfileName
                })
                .ToListAsync();

            var viewModel = new SavedAnalysesIndexVm
            {
                Analyses = analyses
            };

            return View(viewModel);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = CurrentUserId;
            var savedProfile = await _dbContext.SavedPropertyProfiles
                .AsNoTracking()
                .Include(profile => profile.RentalListing)
                .Include(profile => profile.InvestmentProfile)
                .FirstOrDefaultAsync(profile => profile.SavedPropertyProfileId == id
                                                && profile.UserId == userId);

            if (savedProfile is null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return View("NotFound");
            }

            var listing = savedProfile.RentalListing;
            var addressLine = listing.StreetAddress ?? "Unknown address";
            var locationLine = string.Join(", ", new[]
            {
                listing.City,
                listing.State,
                listing.ZipCode
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

            var viewModel = new SavedAnalysisDetailsVm
            {
                SavedPropertyProfileId = savedProfile.SavedPropertyProfileId,
                AddressLine = addressLine,
                LocationLine = locationLine,
                Price = listing.Price,
                SavedAtUtc = savedProfile.SavedAtUtc,
                InvestmentProfileName = savedProfile.InvestmentProfile.InvestmentProfileName,
                Assumptions = new SavedAnalysisAssumptionsVm
                {
                    DownpaymentPercentage = savedProfile.DownpaymentPercentage,
                    MortgageInterestRate = savedProfile.MortgageInterestRate,
                    TermYears = savedProfile.TermYears,
                    ClosingCostOverride = savedProfile.ClosingCostOverride,
                    RenovationBudget = savedProfile.RenovationBudget,
                    OtherUpfrontCosts = savedProfile.OtherUpfrontCosts,
                    MonthlyRentOverride = savedProfile.MonthlyRentOverride,
                    MonthlyOtherExpensesOverride = savedProfile.MonthlyOtherExpensesOverride
                }
            };

            ViewData["CurrentSavedPropertyProfileId"] = savedProfile.SavedPropertyProfileId;
            return View(viewModel);
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
