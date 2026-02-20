using ACM.Models.Dto;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;
using ACM.Services.SleeveManager.Interfaces;
using ACM.Services.StartUpSleeveFetcher.Interfaces;

namespace ACM.Services.StartUpSleeveFetcher
{
    public class StartUpSleeveFetcher : IStartUpSleeveFetcher
    {
        private readonly ISleeveManager _sleeveManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public StartUpSleeveFetcher(
            ISleeveManager sleeveManager,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            _sleeveManager = sleeveManager;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IDeviceManagerClient deviceManagerClient =
                scope.ServiceProvider.GetRequiredService<IDeviceManagerClient>();

            IEnumerable<SleeveDeviceManagerDto> sleeves = await deviceManagerClient.GetSleevesAsync(
                cancellationToken
            );
            _sleeveManager.SaveSleevs(sleeves);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
