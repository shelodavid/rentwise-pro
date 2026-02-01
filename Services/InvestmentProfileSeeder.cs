using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;

namespace RentWisePro.Web.Services
{
    public class InvestmentProfileSeeder
    {
        private readonly RentWiseProDbContext _dbContext;

        public InvestmentProfileSeeder(RentWiseProDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task EnsureSeededAsync(CancellationToken cancellationToken = default)
        {
            var hasProfiles = await _dbContext.InvestmentProfiles.AnyAsync(cancellationToken);
            if (hasProfiles)
            {
                return;
            }

            var defaultProfile = InvestmentProfileDefaults.CreateDefault();
            _dbContext.InvestmentProfiles.Add(defaultProfile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
