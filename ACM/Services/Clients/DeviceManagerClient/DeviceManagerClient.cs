using System.Text.Json;
using System.Net.Http.Json;
using ACM.Common;
using ACM.Models.Dto;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;

namespace ACM.Services.Clients.DeviceManagerClient
{
    public class DeviceManagerClient : IDeviceManagerClient
    {
        private readonly HttpClient _httpClient;

        public DeviceManagerClient(
            IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(ACMConstants.HttpClients.DEVICE_MANAGER_HTTP_CLIENT);
        }

        public async Task AssignSleeveToUavAsync(
            int tailId,
            int sleeveId,
            CancellationToken cancellationToken = default
        )
        {
            AssignSleeveToUavRequestDto request = new AssignSleeveToUavRequestDto(tailId, sleeveId);
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
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
            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNameCaseInsensitive = true,
            };
            IEnumerable<SleeveDeviceManagerDto>? sleeves = await _httpClient.GetFromJsonAsync<
                IEnumerable<SleeveDeviceManagerDto>
            >(
                ACMConstants.DeviceManagerApiEndpoints.GET_ALL_SLEEVES,
                jsonOptions,
                cancellationToken
            );
            return sleeves ?? [];
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
            using HttpResponseMessage response = await _httpClient.PostAsync(
                path,
                null,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();
        }
    }
}
