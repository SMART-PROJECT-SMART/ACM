using ACM.Models.Dto;

namespace ACM.Services.DeviceManagerClient.Interfaces
{
    public interface IDeviceManagerClient
    {
        public Task<IEnumerable<SleeveDeviceManagerDto>> GetSleevesAsync(
            CancellationToken cancellationToken
        );
    }
}
