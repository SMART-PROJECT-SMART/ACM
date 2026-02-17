using System.Net.Http.Json;
using ACM.Common;
using ACM.Models.Dto;
using ACM.Services.DeviceManagerClient.Interfaces;

namespace ACM.Services.DeviceManagerClient
{
    public class DeviceManagerClient : IDeviceManagerClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeviceManagerClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<SleeveDeviceManagerDto>> GetSleevesAsync(CancellationToken cancellationToken)
        {
            using HttpClient client = _httpClientFactory.CreateClient(ACMConstants.HttpClients.DEVICE_MANAGER_HTTP_CLIENT);

            IEnumerable<SleeveDeviceManagerDto>? sleeves = await client.GetFromJsonAsync<IEnumerable<SleeveDeviceManagerDto>>(
                ACMConstants.DeviceManagerApiEndpoints.GET_ALL_SLEEVES,
                cancellationToken
            );

            return sleeves ?? [];
        }
    }
}
