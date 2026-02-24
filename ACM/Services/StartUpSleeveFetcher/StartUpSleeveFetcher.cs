using ACM.Models.Dto;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;
using ACM.Services.SleeveManager.Interfaces;
using ACM.Services.StartUpSleeveFetcher.Interfaces;
using Microsoft.Extensions.Logging;

namespace ACM.Services.StartUpSleeveFetcher
{
    public class StartUpSleeveFetcher : IStartUpSleeveFetcher
    {
        private readonly ISleeveManager _sleeveManager;
        private readonly IDeviceManagerClient _deviceManagerClient;
        private readonly ILogger<StartUpSleeveFetcher> _logger;

        public StartUpSleeveFetcher(
            ISleeveManager sleeveManager,
            IDeviceManagerClient deviceManagerClient,
            ILogger<StartUpSleeveFetcher> logger
        )
        {
            _sleeveManager = sleeveManager;
            _deviceManagerClient = deviceManagerClient;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IEnumerable<SleeveDeviceManagerDto> sleeves = await _deviceManagerClient.GetSleevesAsync(
                cancellationToken
            );
            List<SleeveDeviceManagerDto> sleeveList = sleeves.ToList();
            _logger.LogInformation(
                "Fetched {Count} sleeves from Device Manager: {Details}",
                sleeveList.Count,
                string.Join(", ", sleeveList.Select(s => $"{s.Name} (Id={s.Id})"))
            );
            _sleeveManager.SaveSleeves(sleeveList);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
