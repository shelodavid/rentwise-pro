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

        public async Task<InvestmentProfile?> GetDefaultAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.InvestmentProfiles.AsNoTracking()
                .Where(profile => profile.UserId == userId)
                .OrderByDescending(profile => profile.IsDefault)
                .ThenBy(profile => profile.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
