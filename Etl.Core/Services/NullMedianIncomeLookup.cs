using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Interfaces;

namespace RentWisePro.Etl.Core.Services;

public sealed class NullMedianIncomeLookup : IMedianIncomeLookup
{
    public Task<decimal?> GetMedianMonthlyIncomeAsync(Property property, CancellationToken cancellationToken)
    {
        return Task.FromResult<decimal?>(null);
    }
}
