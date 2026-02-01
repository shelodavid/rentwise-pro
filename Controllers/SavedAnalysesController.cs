using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Domain.Entities;
using RentWisePro.Web.Models.Forecast;
using RentWisePro.Web.Models.SavedAnalyses;
using RentWisePro.Web.Services;

namespace RentWisePro.Web.Controllers
{
    [Route("SavedAnalyses")]
    [Authorize]
    public class SavedAnalysesController : Controller
    {
        private readonly RentWiseProDbContext _dbContext;
        private readonly ForecastCalculationService _forecastCalculationService;
        private readonly ClosingDisclosureCalculationService _closingDisclosureCalculationService;
        private readonly InvestmentProfileResolver _investmentProfileResolver;

        public SavedAnalysesController(
            RentWiseProDbContext dbContext,
            ForecastCalculationService forecastCalculationService,
            ClosingDisclosureCalculationService closingDisclosureCalculationService,
            InvestmentProfileResolver investmentProfileResolver)
        {
            _dbContext = dbContext;
            _forecastCalculationService = forecastCalculationService;
            _closingDisclosureCalculationService = closingDisclosureCalculationService;
            _investmentProfileResolver = investmentProfileResolver;
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
        public async Task<IActionResult> Details(int id, [FromQuery] int? profileId)
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

            var profiles = await _dbContext.InvestmentProfiles
                .AsNoTracking()
                .Where(profile => profile.UserId == userId)
                .OrderBy(profile => profile.InvestmentProfileName)
                .ToListAsync();

            if (!profiles.Any())
            {
                var defaultProfile = await _investmentProfileResolver.EnsureDefaultAsync(userId);
                profiles = new List<InvestmentProfile> { defaultProfile };
            }

            var scenarioProfile = profileId.HasValue
                ? profiles.FirstOrDefault(profile => profile.Id == profileId.Value)
                : null;

            scenarioProfile ??= savedProfile.InvestmentProfile ?? profiles.FirstOrDefault();

            var listing = savedProfile.RentalListing;
            var addressLine = listing.StreetAddress ?? "Unknown address";
            var locationLine = string.Join(", ", new[]
            {
                listing.City,
                listing.State,
                listing.ZipCode
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

            var closingDisclosure = scenarioProfile is null
                ? new ClosingDisclosureSummaryVm()
                : _closingDisclosureCalculationService.BuildSummary(listing, savedProfile, scenarioProfile);

            var viewModel = new SavedAnalysisDetailsVm
            {
                SavedPropertyProfileId = savedProfile.SavedPropertyProfileId,
                AddressLine = addressLine,
                LocationLine = locationLine,
                Price = listing.Price,
                SavedAtUtc = savedProfile.SavedAtUtc,
                SnapshotInvestmentProfileName = savedProfile.InvestmentProfile?.InvestmentProfileName
                    ?? "Snapshot assumptions",
                ScenarioInvestmentProfileName = scenarioProfile?.InvestmentProfileName
                    ?? "Snapshot assumptions",
                ScenarioInvestmentProfileId = scenarioProfile?.Id ?? 0,
                IsScenarioProfileDifferent = scenarioProfile is not null
                                            && savedProfile.InvestmentProfile is not null
                                            && scenarioProfile.Id != savedProfile.InvestmentProfile.Id,
                ScenarioProfiles = profiles.Select(profile => new InvestmentProfileOptionVm
                {
                    Id = profile.Id,
                    Name = profile.InvestmentProfileName,
                    IsDefault = profile.IsDefault
                }).ToList(),
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
                },
                ClosingDisclosure = closingDisclosure
            };

            ViewData["CurrentSavedPropertyProfileId"] = savedProfile.SavedPropertyProfileId;
            return View(viewModel);
        }

        [HttpGet("{id:int}/Forecast")]
        public async Task<IActionResult> Forecast(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

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
            var investmentProfile = savedProfile.InvestmentProfile ?? BuildFallbackProfile(savedProfile, userId);
            var usingFallback = savedProfile.InvestmentProfile is null;
            var calculation = _forecastCalculationService.Calculate(listing, investmentProfile, savedProfile);
            var projections = _forecastCalculationService.BuildHorizonProjections(calculation);

            var addressLine = listing.StreetAddress ?? "Unknown address";
            var locationLine = string.Join(", ", new[]
            {
                listing.City,
                listing.State,
                listing.ZipCode
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

            var viewModel = new SavedAnalysisForecastVm
            {
                SavedPropertyProfileId = savedProfile.SavedPropertyProfileId,
                AddressLine = addressLine,
                LocationLine = locationLine,
                InvestmentProfileName = savedProfile.InvestmentProfile?.InvestmentProfileName ?? "Snapshot assumptions",
                UsingDefaultInvestmentProfile = usingFallback,
                InvestmentProfileNote = usingFallback
                    ? "Vacancy and property management rates defaulted to 0% because no investment profile is attached."
                    : "Vacancy and property management rates are pulled from the saved investment profile.",
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
                SnapshotAssumptions = new SavedAnalysisAssumptionsVm
                {
                    DownpaymentPercentage = savedProfile.DownpaymentPercentage,
                    MortgageInterestRate = savedProfile.MortgageInterestRate,
                    TermYears = savedProfile.TermYears,
                    ClosingCostOverride = savedProfile.ClosingCostOverride,
                    RenovationBudget = savedProfile.RenovationBudget,
                    OtherUpfrontCosts = savedProfile.OtherUpfrontCosts,
                    MonthlyRentOverride = savedProfile.MonthlyRentOverride,
                    MonthlyOtherExpensesOverride = savedProfile.MonthlyOtherExpensesOverride
                },
                Assumptions = calculation.Assumptions,
                Kpis = calculation.Kpis,
                Projections = projections
            };

            ViewData["CurrentSavedPropertyProfileId"] = savedProfile.SavedPropertyProfileId;
            return View(viewModel);
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private static InvestmentProfile BuildFallbackProfile(SavedPropertyProfile savedProfile, string userId)
        {
            return new InvestmentProfile
            {
                InvestmentProfileName = "Snapshot assumptions",
                UserId = userId,
                DownpaymentPercentage = savedProfile.DownpaymentPercentage,
                MortgageInterestRate = savedProfile.MortgageInterestRate,
                TermYears = savedProfile.TermYears,
                PMIRate = 0m,
                PropertyTaxRate = 0m,
                HomeownersInsuranceAnnual = 0m,
                VacancyRate = 0m,
                PropertyManagementFeePct = 0m,
                MonthlyMaintenanceBudget = 0m,
                MonthlyUtilitiesCost = 0m
            };
        }
    }
}
