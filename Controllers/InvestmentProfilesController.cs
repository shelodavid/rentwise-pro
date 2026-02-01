using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Domain.Entities;
using RentWisePro.Web.Models;

namespace RentWisePro.Web.Controllers
{
    [Authorize]
    [Route("InvestmentProfiles")]
    public class InvestmentProfilesController : Controller
    {
        private readonly RentWiseProDbContext _dbContext;

        public InvestmentProfilesController(RentWiseProDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            var profiles = await _dbContext.InvestmentProfiles.AsNoTracking()
                .Where(profile => profile.UserId == userId)
                .OrderByDescending(profile => profile.IsDefault)
                .ThenBy(profile => profile.InvestmentProfileName)
                .ToListAsync();

            return View(profiles);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new InvestmentProfileVm());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestmentProfileVm viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var profile = new InvestmentProfile();
            MapToEntity(viewModel, profile);
            profile.UserId = CurrentUserId;

            _dbContext.InvestmentProfiles.Add(profile);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = CurrentUserId;
            var profile = await _dbContext.InvestmentProfiles.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

            if (profile is null)
            {
                return NotFound();
            }

            var viewModel = MapToViewModel(profile);
            return View(viewModel);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvestmentProfileVm viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var userId = CurrentUserId;
            var profile = await _dbContext.InvestmentProfiles
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

            if (profile is null)
            {
                return NotFound();
            }

            MapToEntity(viewModel, profile);

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("SetDefault/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = CurrentUserId;
            var profile = await _dbContext.InvestmentProfiles
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

            if (profile is null)
            {
                return NotFound();
            }

            await ClearDefaultFlag(profile.Id, userId);
            profile.IsDefault = true;

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task ClearDefaultFlag(int? ignoreId, string userId)
        {
            var defaultsQuery = _dbContext.InvestmentProfiles
                .Where(item => item.UserId == userId && item.IsDefault);
            if (ignoreId.HasValue)
            {
                defaultsQuery = defaultsQuery.Where(item => item.Id != ignoreId.Value);
            }

            var defaults = await defaultsQuery.ToListAsync();
            foreach (var existing in defaults)
            {
                existing.IsDefault = false;
            }
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private static InvestmentProfileVm MapToViewModel(InvestmentProfile profile)
        {
            return new InvestmentProfileVm
            {
                Id = profile.Id,
                InvestmentProfileName = profile.InvestmentProfileName,
                DownpaymentPercentage = profile.DownpaymentPercentage,
                TermYears = profile.TermYears,
                MortgageInterestRate = profile.MortgageInterestRate,
                PMIRate = profile.PMIRate,
                PropertyTaxRate = profile.PropertyTaxRate,
                HomeownersInsuranceAnnual = profile.HomeownersInsuranceAnnual,
                VacancyRate = profile.VacancyRate ?? 0m,
                PropertyManagementFeePct = profile.PropertyManagementFeePct ?? 0m,
                MonthlyMaintenanceBudget = profile.MonthlyMaintenanceBudget ?? 0m,
                MonthlyUtilitiesCost = profile.MonthlyUtilitiesCost ?? 0m,
                RealtorClosingFeePercentage = profile.RealtorClosingFeePercentage ?? 0m,
                ClosingCostsPercentage = profile.ClosingCostsPercentage ?? 0m,
                LoanOriginationFeePct = profile.LoanOriginationFeePct ?? 0m,
                AppraisalFee = profile.AppraisalFee ?? 0m,
                CreditReportFee = profile.CreditReportFee ?? 0m,
                TitleInsuranceCost = profile.TitleInsuranceCost ?? 0m,
                TitleSearchFee = profile.TitleSearchFee ?? 0m,
                EscrowFee = profile.EscrowFee ?? 0m,
                FloodInspectionFee = profile.FloodInspectionFee ?? 0m,
                MiscellaneousFees = profile.MiscellaneousFees ?? 0m,
                HOAEstimate = profile.HOAEstimate ?? 0m
            };
        }

        private static void MapToEntity(InvestmentProfileVm viewModel, InvestmentProfile profile)
        {
            profile.InvestmentProfileName = viewModel.InvestmentProfileName;
            profile.DownpaymentPercentage = viewModel.DownpaymentPercentage;
            profile.TermYears = viewModel.TermYears;
            profile.MortgageInterestRate = viewModel.MortgageInterestRate;
            profile.PMIRate = viewModel.PMIRate;
            profile.PropertyTaxRate = viewModel.PropertyTaxRate;
            profile.HomeownersInsuranceAnnual = viewModel.HomeownersInsuranceAnnual;
            profile.VacancyRate = viewModel.VacancyRate ?? 0m;
            profile.PropertyManagementFeePct = viewModel.PropertyManagementFeePct ?? 0m;
            profile.MonthlyMaintenanceBudget = viewModel.MonthlyMaintenanceBudget ?? 0m;
            profile.MonthlyUtilitiesCost = viewModel.MonthlyUtilitiesCost ?? 0m;
            profile.RealtorClosingFeePercentage = viewModel.RealtorClosingFeePercentage ?? 0m;
            profile.ClosingCostsPercentage = viewModel.ClosingCostsPercentage ?? 0m;
            profile.LoanOriginationFeePct = viewModel.LoanOriginationFeePct ?? 0m;
            profile.AppraisalFee = viewModel.AppraisalFee ?? 0m;
            profile.CreditReportFee = viewModel.CreditReportFee ?? 0m;
            profile.TitleInsuranceCost = viewModel.TitleInsuranceCost ?? 0m;
            profile.TitleSearchFee = viewModel.TitleSearchFee ?? 0m;
            profile.EscrowFee = viewModel.EscrowFee ?? 0m;
            profile.FloodInspectionFee = viewModel.FloodInspectionFee ?? 0m;
            profile.MiscellaneousFees = viewModel.MiscellaneousFees ?? 0m;
            profile.HOAEstimate = viewModel.HOAEstimate ?? 0m;
        }
    }
}
