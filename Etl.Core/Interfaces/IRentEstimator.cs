using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Models;

namespace RentWisePro.Etl.Core.Interfaces;

public interface IRentEstimator
{
    int Priority { get; }
    Task<RentEstimate?> EstimateAsync(Property property, CancellationToken cancellationToken);
}
