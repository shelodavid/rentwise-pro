using RentWisePro.Etl.Core.Models;

namespace RentWisePro.Etl.Core.Interfaces;

public interface IEtlOrchestrator
{
    Task RunAsync(EtlRunRequest request, CancellationToken cancellationToken);
}
