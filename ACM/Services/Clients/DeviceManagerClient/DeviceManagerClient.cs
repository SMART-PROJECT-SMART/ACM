using System.Net.Http.Json;
using ACM.Common;
using ACM.Models.Dto;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;
using Microsoft.Extensions.Logging;

namespace ACM.Services.Clients.DeviceManagerClient
{
    public class DeviceManagerClient : IDeviceManagerClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeviceManagerClient> _logger;

        public DeviceManagerClient(HttpClient httpClient, ILogger<DeviceManagerClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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
                try
                {
                    await AssignSleeveToUavAsync(change.TailId, change.SleeveId, cancellationToken);
                    _logger.LogInformation(
                        "Changed sleeve for UAV tail {TailId} to sleeve {SleeveName} (id {SleeveId})",
                        change.TailId,
                        change.SleeveName,
                        change.SleeveId
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to assign sleeve {SleeveName} (id {SleeveId}) to tail {TailId}",
                        change.SleeveName,
                        change.SleeveId,
                        change.TailId
                    );
                    throw;
                }
            }

            foreach (int tailId in removedTailIds)
            {
                try
                {
                    await ReleaseSleeveByTailIdAsync(tailId, cancellationToken);
                    _logger.LogInformation("Released sleeve for UAV tail {TailId}", tailId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to release sleeve for tail {TailId}", tailId);
                    throw;
                }
            }
        }

        public async Task<IEnumerable<SleeveDeviceManagerDto>> GetSleevesAsync(
            CancellationToken cancellationToken = default
        )
        {
            IEnumerable<SleeveDeviceManagerDto>? sleeves = await _httpClient.GetFromJsonAsync<
                IEnumerable<SleeveDeviceManagerDto>
            >(ACMConstants.DeviceManagerApiEndpoints.GET_ALL_SLEEVES, cancellationToken);
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
