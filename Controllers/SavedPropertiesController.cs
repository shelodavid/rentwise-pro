using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Domain.Entities;
using RentWisePro.Web.Models;
using RentWisePro.Web.Services;

namespace RentWisePro.Web.Controllers
{
    [Authorize]
    [Route("SavedProperties")]
    public class SavedPropertiesController : Controller
    {
        private readonly RentWiseProDbContext _dbContext;
        private readonly InvestmentProfileResolver _investmentProfileResolver;

        public SavedPropertiesController(RentWiseProDbContext dbContext, InvestmentProfileResolver investmentProfileResolver)
        {
            _dbContext = dbContext;
            _investmentProfileResolver = investmentProfileResolver;
        }

        [HttpPost("StartAnalysis")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartAnalysis(StartAnalysisRequest request)
        {
            if (request == null || (!request.Zpid.HasValue && !request.RentalListingId.HasValue))
            {
                return BadRequest("A listing identifier is required.");
            }

            var listingQuery = _dbContext.RentalListings.AsNoTracking();
            var listing = request.RentalListingId.HasValue
                ? await listingQuery.FirstOrDefaultAsync(item => item.RentalListingId == request.RentalListingId.Value)
                : await listingQuery.FirstOrDefaultAsync(item => item.Zpid == request.Zpid!.Value);

            if (listing is null)
            {
                return NotFound("Listing not found.");
            }

            var userId = CurrentUserId;
            var investmentProfile = await _investmentProfileResolver.GetDefaultAsync(userId);

            if (investmentProfile is null)
            {
                return NotFound("Default investment profile not found.");
            }

            var savedProfile = await _dbContext.SavedPropertyProfiles
                .FirstOrDefaultAsync(profile => profile.RentalListingId == listing.RentalListingId
                                                && profile.InvestmentProfileId == investmentProfile.Id
                                                && profile.UserId == userId);

            if (savedProfile is null)
            {
                savedProfile = new SavedPropertyProfile
                {
                    InvestmentProfileId = investmentProfile.Id,
                    RentalListingId = listing.RentalListingId,
                    DownpaymentPercentage = investmentProfile.DownpaymentPercentage,
                    MortgageInterestRate = investmentProfile.MortgageInterestRate,
                    TermYears = investmentProfile.TermYears,
                    SavedAtUtc = DateTime.UtcNow,
                    UserId = userId
                };

                _dbContext.SavedPropertyProfiles.Add(savedProfile);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("GeneratePurchaseSheet", "Home",
                new { savedPropertyProfileId = savedProfile.SavedPropertyProfileId });
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
