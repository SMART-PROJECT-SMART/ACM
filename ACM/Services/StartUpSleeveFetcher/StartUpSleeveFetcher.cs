using ACM.Services.SleeveManager.Interfaces;
using ACM.Services.StartUpSleeveFetcher.Interfaces;

namespace ACM.Services.StartUpSleeveFetcher
{
    public class StartUpSleeveFetcher : IStartUpSleeveFetcher
    {
        public StartUpSleeveFetcher(ISleeveManager sleeveManager)
        {
            _sleeveManager = sleeveManager;
        }

        private readonly ISleeveManager _sleeveManager;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
