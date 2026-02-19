using ACM.Models.Dto;

namespace ACM.Services.DeviceManagerClient.Interfaces
{
    public interface IDeviceManagerClient
    {
        Task AssignSleeveToUavAsync(int tailId, string sleeveName, CancellationToken cancellationToken = default);
        Task<IEnumerable<SleeveDeviceManagerDto>> GetSleevesAsync(CancellationToken cancellationToken = default);
        Task ReleaseSleeveByTailIdAsync(int tailId, CancellationToken cancellationToken = default);
    }
}
