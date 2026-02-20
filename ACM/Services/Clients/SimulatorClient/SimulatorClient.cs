using System.Net.Http.Json;
using ACM.Common;
using ACM.Models.Dto;
using ACM.Services.Clients.SimulatorClient.Interfaces;

namespace ACM.Services.Clients.SimulatorClient
{
    public class SimulatorClient : ISimulatorClient
    {
        private readonly HttpClient _httpClient;

        public SimulatorClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task NotifyUavPortsChangedAsync(
            int tailId,
            IEnumerable<int> newPorts,
            CancellationToken cancellationToken = default
        )
        {
            var request = new UavPortsChangedRequestDto { TailId = tailId, NewPorts = newPorts };
            await _httpClient.PostAsJsonAsync(
                ACMConstants.SimulationApiEndpoints.UAV_PORTS_CHANGED,
                request,
                cancellationToken
            );
        }
    }
}
