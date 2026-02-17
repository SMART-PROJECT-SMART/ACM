using ACM.Models.Dto;
using ACM.Services.SleeveChangeHandlers.Interfaces;
using ACM.Services.SleeveManager.Interfaces;
using Core.Common.Enums;

namespace ACM.Services.SleeveChangeHandlers.Handlers
{
    public class SleeveDeletedHandler : ISleeveChangeHandler
    {
        private readonly ISleeveManager _sleeveManager;

        public SleeveDeletedHandler(ISleeveManager sleeveManager)
        {
            _sleeveManager = sleeveManager;
        }

        public bool CanHandle(CrudOperation operation) => operation is CrudOperation.Deleted;

        public Task HandleSleeveChangeAsync(string name, CancellationToken cancellationToken = default)
        {
            _sleeveManager.DeleteSleeves(new DeleteSleeveDto
            {
                SleevsToDelete = new[] { name }
            });

            return Task.CompletedTask;
        }
    }
}
