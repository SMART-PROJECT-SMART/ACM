using ACM.Models.Dto;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;
using ACM.Services.SleeveManager.Interfaces;
using ACM.Services.StartUpSleeveFetcher.Interfaces;

namespace ACM.Services.StartUpSleeveFetcher
{
    public class StartUpSleeveFetcher : IStartUpSleeveFetcher
    {
        private readonly ISleeveManager _sleeveManager;
        private readonly IDeviceManagerClient _deviceManagerClient;

        public StartUpSleeveFetcher(
            ISleeveManager sleeveManager,
            IDeviceManagerClient deviceManagerClient
        )
        {
            _sleeveManager = sleeveManager;
            _deviceManagerClient = deviceManagerClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IEnumerable<SleeveDeviceManagerDto> sleeves = await _deviceManagerClient.GetSleevesAsync(
                cancellationToken
            );
            List<SleeveDeviceManagerDto> sleeveList = sleeves.ToList();
            _sleeveManager.SaveSleeves(sleeveList);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
