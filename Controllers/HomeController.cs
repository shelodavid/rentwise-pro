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
        public async Task<IActionResult> GeneratePurchaseSheet(long zpid)
        {
            if (zpid <= 0)
            {
                return BadRequest();
            }

            var listing = await _dbContext.RentalListings
                .FirstOrDefaultAsync(item => item.Zpid == zpid);

            if (listing == null)
            {
                return NotFound();
            }

            var profile = await _dbContext.InvestmentProfiles
                .FirstOrDefaultAsync(item => item.Id == 1);

            if (profile == null)
            {
                return NotFound();
            }

            var savedProfile = await _dbContext.SavedPropertyProfiles
                .FirstOrDefaultAsync(item =>
                    item.InvestmentProfileId == profile.Id &&
                    item.RentalListingId == listing.RentalListingId);

            if (savedProfile == null)
            {
                savedProfile = new SavedPropertyProfile
                {
                    InvestmentProfileId = profile.Id,
                    RentalListingId = listing.RentalListingId,
                    DownpaymentPercentage = profile.DownpaymentPercentage,
                    MortgageInterestRate = profile.MortgageInterestRate,
                    TermYears = profile.TermYears
                };

                _dbContext.SavedPropertyProfiles.Add(savedProfile);
                await _dbContext.SaveChangesAsync();
            }

            var viewModel = BuildPurchaseSheetViewModel(listing, profile, savedProfile);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePurchaseSheet(PurchaseSheetPageVm model)
        {
            var listing = await _dbContext.RentalListings
                .FirstOrDefaultAsync(item => item.Zpid == model.Zpid);

            if (listing == null)
            {
                return NotFound();
            }

            var profile = await _dbContext.InvestmentProfiles
                .FirstOrDefaultAsync(item => item.Id == 1);

            if (profile == null)
            {
                return NotFound();
            }

            var savedProfile = await _dbContext.SavedPropertyProfiles
                .FirstOrDefaultAsync(item =>
                    item.InvestmentProfileId == profile.Id &&
                    item.RentalListingId == listing.RentalListingId);

            if (!ModelState.IsValid)
            {
                var fallbackProfile = savedProfile ?? new SavedPropertyProfile
                {
                    InvestmentProfileId = profile.Id,
                    RentalListingId = listing.RentalListingId
                };

                ApplyOverrides(model, fallbackProfile);
                var invalidViewModel = BuildPurchaseSheetViewModel(listing, profile, fallbackProfile);
                return View(invalidViewModel);
            }

            if (savedProfile == null)
            {
                savedProfile = new SavedPropertyProfile
                {
                    InvestmentProfileId = profile.Id,
                    RentalListingId = listing.RentalListingId
                };
                _dbContext.SavedPropertyProfiles.Add(savedProfile);
            }

            ApplyOverrides(model, savedProfile);
            savedProfile.SavedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var viewModel = BuildPurchaseSheetViewModel(listing, profile, savedProfile);
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
    }
}
