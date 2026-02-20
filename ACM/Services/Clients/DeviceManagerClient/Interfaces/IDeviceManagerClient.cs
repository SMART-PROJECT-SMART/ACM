using ACM.Models.Dto;

namespace ACM.Services.Clients.DeviceManagerClient.Interfaces
{
    public interface IDeviceManagerClient
    {
        Task AssignSleeveToUavAsync(
            int tailId,
            int sleeveId,
            CancellationToken cancellationToken = default
        );
        Task ApplyAssignmentChangesAsync(
            IReadOnlyList<ChangedAssignmentDto> changedAssignments,
            IReadOnlySet<int> removedTailIds,
            CancellationToken cancellationToken = default
        );
        Task<IEnumerable<SleeveDeviceManagerDto>> GetSleevesAsync(
            CancellationToken cancellationToken = default
        );
        Task ReleaseSleeveByTailIdAsync(int tailId, CancellationToken cancellationToken = default);
    }
}
