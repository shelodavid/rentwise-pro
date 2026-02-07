using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;

namespace RentWisePro.Web.Controllers
{
    [Route("diagnostics")]
    public class DiagnosticsController : Controller
    {
        private readonly RentWiseProDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public DiagnosticsController(RentWiseProDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpGet("db")]
        public async Task<IActionResult> Db()
        {
            // Basic connectivity + can we query?
            var canConnect = await _db.Database.CanConnectAsync();
            var profileCount = await _db.InvestmentProfiles.CountAsync();
            var listingCount = await _db.RentalListings.CountAsync();
            var savedCount = await _db.SavedPropertyProfiles.CountAsync();
            var count = _db.RentalListings.Count();

            return Ok(new
            {
                canConnect,
                counts = new
                {
                    investmentProfiles = profileCount,
                    rentalListings = listingCount,
                    savedPropertyProfiles = savedCount,
                    listingCount = count
                }
            });
        }

        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            var roles = User.Claims
                .Where(claim => claim.Type == ClaimTypes.Role)
                .Select(claim => claim.Value)
                .ToArray();

            return Ok(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                name = User.Identity?.Name,
                email = User.FindFirstValue(ClaimTypes.Email),
                roles,
                claims = User.Claims.Select(claim => new { claim.Type, claim.Value })
            });
        }
    }
}
