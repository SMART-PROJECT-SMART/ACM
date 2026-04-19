using ACM.Models.Dto;

namespace ACM.Services.Clients.SimulatorClient.Interfaces
{
    public interface ISimulatorClient
    {
        Task NotifyUavPortsChangedAsync(
            int tailId,
            IEnumerable<int> newPorts,
            CancellationToken cancellationToken = default
        );
        Task NotifyUavPortsChangedBatchAsync(
            UavPortsChangedBatchRequestDto request,
            CancellationToken cancellationToken = default
        );
    }
}
