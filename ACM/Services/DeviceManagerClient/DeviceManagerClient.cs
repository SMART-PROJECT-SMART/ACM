using System.Net.Http.Json;
using ACM.Common;
using ACM.Models.Dto;
using ACM.Services.DeviceManagerClient.Interfaces;

namespace ACM.Services.DeviceManagerClient
{
    public class DeviceManagerClient : IDeviceManagerClient
    {
        private readonly HttpClient _httpClient;

        public DeviceManagerClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AssignSleeveToUavAsync(int tailId, string sleeveName, CancellationToken cancellationToken = default)
        {
            AssignSleeveToUavRequestDto request = new AssignSleeveToUavRequestDto(tailId, sleeveName);
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                ACMConstants.DeviceManagerApiEndpoints.ASSIGN_SLEEVE_TO_UAV,
                request,
                cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<SleeveDeviceManagerDto>> GetSleevesAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<SleeveDeviceManagerDto>? sleeves = await _httpClient.GetFromJsonAsync<IEnumerable<SleeveDeviceManagerDto>>(
                ACMConstants.DeviceManagerApiEndpoints.GET_ALL_SLEEVES,
                cancellationToken);
            return sleeves ?? [];
        }

        public async Task ReleaseSleeveByTailIdAsync(int tailId, CancellationToken cancellationToken = default)
        {
            string path = string.Format(ACMConstants.DeviceManagerApiEndpoints.RELEASE_SLEEVE_BY_TAIL_ID, tailId);
            using HttpResponseMessage response = await _httpClient.PostAsync(path, null, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
