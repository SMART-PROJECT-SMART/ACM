using System.Net.Http.Json;
using ACM.Common;
using ACM.Models.Dto;
using ACM.Services.Clients.SimulatorClient.Interfaces;

namespace ACM.Services.Clients.SimulatorClient
{
    public class SimulatorClient : ISimulatorClient
    {
        private readonly HttpClient _httpClient;

        public SimulatorClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(
                ACMConstants.HttpClients.SIMULATOR_HTTP_CLIENT
            );
        }

        public async Task NotifyUavPortsChangedAsync(
            int tailId,
            IEnumerable<int> newPorts,
            CancellationToken cancellationToken = default
        )
        {
            UavPortsChangedRequestDto request = new() { TailId = tailId, NewPorts = newPorts };
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                ACMConstants.SimulationApiEndpoints.UAV_PORTS_CHANGED,
                request,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();
        }
    }
}
