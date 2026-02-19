namespace ACM.Services.SimulatorClient.Interfaces
{
    public interface ISimulatorClient
    {
        Task NotifyUavPortsChangedAsync(
            int tailId,
            IEnumerable<int> newPorts,
            CancellationToken cancellationToken = default);
    }
}
