using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Domain.Entities;
using RentWisePro.Web.Models;
using RentWisePro.Web.Services;

namespace RentWisePro.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RentWiseProDbContext _dbContext;
        private readonly PurchaseSheetCalculationService _calculationService;

        public HomeController(
            ILogger<HomeController> logger,
            RentWiseProDbContext dbContext,
            PurchaseSheetCalculationService calculationService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _calculationService = calculationService;
        }

        public async Task<IActionResult> Index()
        {
            var listings = await _dbContext.RentalListings
                .AsNoTracking()
                .OrderByDescending(listing => listing.IngestedAtUtc)
                .Take(24)
                .ToListAsync();

            var viewModel = new HomeIndexVm
            {
                Listings = listings.Select(listing => new PurchaseSheetListingVm
                {
                    Zpid = listing.Zpid,
                    StreetAddress = listing.StreetAddress,
                    City = listing.City,
                    State = listing.State,
                    ZipCode = listing.ZipCode,
                    Price = listing.Price,
                    Bedrooms = listing.Bedrooms,
                    Bathrooms = listing.Bathrooms,
                    ImgSrc = listing.ImgSrc
                }).ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePurchaseSheet(int? savedPropertyProfileId, int? savedProfileId)
        {
            var resolvedProfileId = savedProfileId ?? savedPropertyProfileId;
            if (!resolvedProfileId.HasValue || resolvedProfileId.Value <= 0)
            {
                return RedirectToListingsWithMessage();
            }

            var dataBundle = await LoadPurchaseSheetData(resolvedProfileId.Value, trackSavedProfile: false);
            if (dataBundle.SavedProfile is null)
            {
                return RedirectToListingsWithMessage();
            }

            if (dataBundle.Listing is null || dataBundle.Profile is null)
            {
                return NotFound();
            }

            var viewModel = BuildPurchaseSheetViewModel(dataBundle.Listing, dataBundle.Profile, dataBundle.SavedProfile);
            ViewData["CurrentSavedPropertyProfileId"] = dataBundle.SavedProfile.SavedPropertyProfileId;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePurchaseSheet(PurchaseSheetPageVm model)
        {
            if (model.SavedPropertyProfileId <= 0)
            {
                return RedirectToListingsWithMessage();
            }

            var dataBundle = await LoadPurchaseSheetData(model.SavedPropertyProfileId, trackSavedProfile: true);
            if (dataBundle.SavedProfile is null)
            {
                return RedirectToListingsWithMessage();
            }

            if (dataBundle.Listing is null || dataBundle.Profile is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var fallbackProfile = CloneSavedProfile(dataBundle.SavedProfile);

                ApplyOverrides(model, fallbackProfile);
                var invalidViewModel = BuildPurchaseSheetViewModel(dataBundle.Listing, dataBundle.Profile, fallbackProfile);
                ViewData["CurrentSavedPropertyProfileId"] = dataBundle.SavedProfile.SavedPropertyProfileId;
                return View(invalidViewModel);
            }

            ApplyOverrides(model, dataBundle.SavedProfile);
            dataBundle.SavedProfile.SavedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var viewModel = BuildPurchaseSheetViewModel(dataBundle.Listing, dataBundle.Profile, dataBundle.SavedProfile);
            ViewData["CurrentSavedPropertyProfileId"] = dataBundle.SavedProfile.SavedPropertyProfileId;
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void ApplyOverrides(PurchaseSheetPageVm model, SavedPropertyProfile profile)
        {
            profile.DownpaymentPercentage = model.DownpaymentPercentage;
            profile.MortgageInterestRate = model.MortgageInterestRate;
            profile.TermYears = model.TermYears;
            profile.ClosingCostOverride = model.ClosingCostOverride;
            profile.RenovationBudget = model.RenovationBudget;
            profile.OtherUpfrontCosts = model.OtherUpfrontCosts;
        }

        private PurchaseSheetPageVm BuildPurchaseSheetViewModel(
            RentalListing listing,
            InvestmentProfile profile,
            SavedPropertyProfile savedProfile)
        {
            var result = _calculationService.Calculate(listing, profile, savedProfile);

            return new PurchaseSheetPageVm
            {
                SavedPropertyProfileId = savedProfile.SavedPropertyProfileId,
                Zpid = listing.Zpid,
                RentalListingId = listing.RentalListingId,
                StreetAddress = listing.StreetAddress,
                City = listing.City,
                State = listing.State,
                ZipCode = listing.ZipCode,
                PropertyType = listing.PropertyType,
                Price = listing.Price,
                EstimatedRent = listing.EstimatedRent,
                Bedrooms = listing.Bedrooms,
                Bathrooms = listing.Bathrooms,
                ImgSrc = listing.ImgSrc,
                DownpaymentPercentage = savedProfile.DownpaymentPercentage,
                MortgageInterestRate = savedProfile.MortgageInterestRate,
                TermYears = savedProfile.TermYears,
                ClosingCostOverride = savedProfile.ClosingCostOverride,
                RenovationBudget = savedProfile.RenovationBudget,
                OtherUpfrontCosts = savedProfile.OtherUpfrontCosts,
                Outputs = new PurchaseSheetOutputsVm
                {
                    DownpaymentAmount = result.DownpaymentAmount,
                    MortgageAmount = result.MortgageAmount,
                    MonthlyPrincipalAndInterest = result.MonthlyPrincipalAndInterest,
                    TotalClosingCosts = result.TotalClosingCosts,
                    CalculatedClosingCosts = result.CalculatedClosingCosts,
                    CashToClose = result.CashToClose
                },
                ClosingCosts = new PurchaseSheetClosingCostsVm
                {
                    ClosingCostsPercentage = result.ClosingCosts.ClosingCostsPercentage,
                    RealtorFees = result.ClosingCosts.RealtorFees,
                    LoanOriginationFee = result.ClosingCosts.LoanOriginationFee,
                    AppraisalFee = result.ClosingCosts.AppraisalFee,
                    CreditReportFee = result.ClosingCosts.CreditReportFee,
                    TitleInsurance = result.ClosingCosts.TitleInsurance,
                    TitleSearch = result.ClosingCosts.TitleSearch,
                    EscrowFee = result.ClosingCosts.EscrowFee,
                    FloodInspectionFee = result.ClosingCosts.FloodInspectionFee,
                    MiscellaneousFees = result.ClosingCosts.MiscellaneousFees,
                    HoaEstimate = result.ClosingCosts.HoaEstimate
                }
            };
        }

        private IActionResult RedirectToListingsWithMessage()
        {
            TempData["StatusMessage"] = "Start an analysis first.";
            return RedirectToAction("Index");
        }

        private SavedPropertyProfile CloneSavedProfile(SavedPropertyProfile savedProfile)
        {
            return new SavedPropertyProfile
            {
                SavedPropertyProfileId = savedProfile.SavedPropertyProfileId,
                InvestmentProfileId = savedProfile.InvestmentProfileId,
                RentalListingId = savedProfile.RentalListingId,
                DownpaymentPercentage = savedProfile.DownpaymentPercentage,
                MortgageInterestRate = savedProfile.MortgageInterestRate,
                TermYears = savedProfile.TermYears,
                ClosingCostOverride = savedProfile.ClosingCostOverride,
                RenovationBudget = savedProfile.RenovationBudget,
                OtherUpfrontCosts = savedProfile.OtherUpfrontCosts,
                MonthlyRentOverride = savedProfile.MonthlyRentOverride,
                MonthlyOtherExpensesOverride = savedProfile.MonthlyOtherExpensesOverride,
                SavedAtUtc = savedProfile.SavedAtUtc
            };
        }

        private async Task<(SavedPropertyProfile? SavedProfile, RentalListing? Listing, InvestmentProfile? Profile)>
            LoadPurchaseSheetData(int savedPropertyProfileId, bool trackSavedProfile)
        {
            IQueryable<SavedPropertyProfile> savedProfileQuery = _dbContext.SavedPropertyProfiles;
            if (!trackSavedProfile)
            {
                savedProfileQuery = savedProfileQuery.AsNoTracking();
            }

            var savedProfile = await savedProfileQuery
                .FirstOrDefaultAsync(item => item.SavedPropertyProfileId == savedPropertyProfileId);

            if (savedProfile is null)
            {
                return (null, null, null);
            }

            var listing = await _dbContext.RentalListings.AsNoTracking()
                .FirstOrDefaultAsync(item => item.RentalListingId == savedProfile.RentalListingId);

            var profile = await _dbContext.InvestmentProfiles.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == savedProfile.InvestmentProfileId);

            return (savedProfile, listing, profile);
        }
    }
}
