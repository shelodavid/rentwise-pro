using RentWisePro.Etl.Core.Services;

namespace RentWisePro.Etl.Core.Interfaces;

public interface IEtlOrchestrator
{
    Task RunAsync(EtlRunRequest request, CancellationToken cancellationToken);
}
