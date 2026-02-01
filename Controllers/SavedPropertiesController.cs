using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Domain.Entities;
using RentWisePro.Web.Models;

namespace RentWisePro.Web.Controllers
{
    [Route("SavedProperties")]
    [Authorize]
    public class SavedPropertiesController : Controller
    {
        private readonly RentWiseProDbContext _dbContext;

        public SavedPropertiesController(RentWiseProDbContext dbContext)
        {
            _dbContext = dbContext;
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

            var investmentProfile = await _dbContext.InvestmentProfiles
                .FirstOrDefaultAsync(profile => profile.Id == 1);

            if (investmentProfile is null)
            {
                return NotFound("Default investment profile not found.");
            }

            var savedProfile = await _dbContext.SavedPropertyProfiles
                .FirstOrDefaultAsync(profile => profile.RentalListingId == listing.RentalListingId
                                                && profile.InvestmentProfileId == investmentProfile.Id);

            if (savedProfile is null)
            {
                savedProfile = new SavedPropertyProfile
                {
                    InvestmentProfileId = investmentProfile.Id,
                    RentalListingId = listing.RentalListingId,
                    DownpaymentPercentage = investmentProfile.DownpaymentPercentage,
                    MortgageInterestRate = investmentProfile.MortgageInterestRate,
                    TermYears = investmentProfile.TermYears,
                    SavedAtUtc = DateTime.UtcNow
                };

                _dbContext.SavedPropertyProfiles.Add(savedProfile);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("GeneratePurchaseSheet", "Home",
                new { savedPropertyProfileId = savedProfile.SavedPropertyProfileId });
        }
    }
}
