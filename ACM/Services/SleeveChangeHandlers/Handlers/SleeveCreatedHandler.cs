using ACM.Models.Dto;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;
using ACM.Services.SleeveChangeHandlers.Interfaces;
using ACM.Services.SleeveManager.Interfaces;
using Core.Common.Enums;

namespace ACM.Services.SleeveChangeHandlers.Handlers
{
    public class SleeveCreatedHandler : ISleeveChangeHandler
    {
        private readonly ISleeveManager _sleeveManager;
        private readonly IDeviceManagerClient _deviceManagerClient;

        public SleeveCreatedHandler(
            ISleeveManager sleeveManager,
            IDeviceManagerClient deviceManagerClient
        )
        {
            _sleeveManager = sleeveManager;
            _deviceManagerClient = deviceManagerClient;
        }

        public bool CanHandle(CrudOperation operation) => operation is CrudOperation.Created;

        public async Task HandleSleeveChangeAsync(
            int id,
            CancellationToken cancellationToken = default
        )
        {
            IEnumerable<SleeveDeviceManagerDto> sleeves =
                await _deviceManagerClient.GetSleevesAsync(cancellationToken);
            SleeveDeviceManagerDto? sleeve = sleeves.FirstOrDefault(s => s.Id == id);

            if (sleeve != null)
            {
                _sleeveManager.SaveSleeves(new[] { sleeve });
            }
        }
    }
}
