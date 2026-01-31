using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;

namespace RentWisePro.Web.Controllers
{
    [Route("diagnostics")]
    public class DiagnosticsController : Controller
    {
        private readonly RentWiseProDbContext _db;

        public DiagnosticsController(RentWiseProDbContext db)
        {
            _db = db;
        }

        [HttpGet("db")]
        public async Task<IActionResult> Db()
        {
            // Basic connectivity + can we query?
            var canConnect = await _db.Database.CanConnectAsync();
            var profileCount = await _db.InvestmentProfiles.CountAsync();
            var listingCount = await _db.RentalListings.CountAsync();
            var savedCount = await _db.SavedPropertyProfiles.CountAsync();

            return Ok(new
            {
                canConnect,
                counts = new
                {
                    investmentProfiles = profileCount,
                    rentalListings = listingCount,
                    savedPropertyProfiles = savedCount
                }
            });
        }
    }
}
