using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Domain.Entities;

namespace RentWisePro.Web.Services
{
    public class InvestmentProfileResolver
    {
        private readonly RentWiseProDbContext _dbContext;

        public InvestmentProfileResolver(RentWiseProDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<InvestmentProfile> EnsureDefaultAsync(string userId, CancellationToken cancellationToken = default)
        {
            var existing = await _dbContext.InvestmentProfiles
                .Where(profile => profile.UserId == userId && profile.IsDefault)
                .OrderBy(profile => profile.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing is not null)
            {
                return existing;
            }

            var defaultProfile = InvestmentProfileDefaults.CreateDefault();
            defaultProfile.UserId = userId;
            defaultProfile.IsDefault = true;

            _dbContext.InvestmentProfiles.Add(defaultProfile);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return defaultProfile;
            }
            catch (DbUpdateException)
            {
                var fallback = await _dbContext.InvestmentProfiles.AsNoTracking()
                    .Where(profile => profile.UserId == userId && profile.IsDefault)
                    .OrderBy(profile => profile.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (fallback is not null)
                {
                    return fallback;
                }

                throw;
            }
        }
    }
}
