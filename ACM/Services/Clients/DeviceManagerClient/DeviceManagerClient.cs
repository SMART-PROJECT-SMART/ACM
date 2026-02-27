using System.Net.Http.Json;
using System.Text.Json;
using ACM.Common;
using ACM.Models.Dto;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;

namespace ACM.Services.Clients.DeviceManagerClient
{
    public class DeviceManagerClient : IDeviceManagerClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DeviceManagerClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(
                ACMConstants.HttpClients.DEVICE_MANAGER_HTTP_CLIENT
            );
            _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        }

        public async Task AssignSleeveToUavAsync(
            int tailId,
            int sleeveId,
            CancellationToken cancellationToken = default
        )
        {
            AssignSleeveToUavRequestDto request = new(tailId, sleeveId);
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                ACMConstants.DeviceManagerApiEndpoints.ASSIGN_SLEEVE_TO_UAV,
                request,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();
        }

        public async Task ApplyAssignmentChangesAsync(
            IReadOnlyList<ChangedAssignmentDto> changedAssignments,
            IReadOnlySet<int> removedTailIds,
            CancellationToken cancellationToken = default
        )
        {
            foreach (ChangedAssignmentDto change in changedAssignments)
            {
                await AssignSleeveToUavAsync(change.TailId, change.SleeveId, cancellationToken);
            }

            foreach (int tailId in removedTailIds)
            {
                await ReleaseSleeveByTailIdAsync(tailId, cancellationToken);
            }
        }

        public async Task<IEnumerable<SleeveDeviceManagerDto>> GetSleevesAsync(
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                IEnumerable<SleeveDeviceManagerDto>? sleeves = await _httpClient.GetFromJsonAsync<
                    IEnumerable<SleeveDeviceManagerDto>
                >(
                    ACMConstants.DeviceManagerApiEndpoints.GET_ALL_SLEEVES,
                    _jsonSerializerOptions,
                    cancellationToken
                );
                return sleeves ?? [];
            }
            catch (Exception)
            {
                return [];
            }
        }

        public async Task ReleaseSleeveByTailIdAsync(
            int tailId,
            CancellationToken cancellationToken = default
        )
        {
            string path = string.Format(
                ACMConstants.DeviceManagerApiEndpoints.RELEASE_SLEEVE_BY_TAIL_ID,
                tailId
            );
            HttpResponseMessage response = await _httpClient.PostAsync(
                path,
                null,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();
        }
    }
}
