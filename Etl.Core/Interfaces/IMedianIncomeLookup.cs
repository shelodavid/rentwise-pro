using RentWisePro.Etl.Core.Entities;

namespace RentWisePro.Etl.Core.Interfaces;

public interface IMedianIncomeLookup
{
    Task<decimal?> GetMedianMonthlyIncomeAsync(Property property, CancellationToken cancellationToken);
}
