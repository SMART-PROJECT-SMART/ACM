using Core.Common.Enums;

namespace ACM.Services.SleeveChangeHandlers.Interfaces
{
    public interface ISleeveChangeHandler
    {
        bool CanHandle(CrudOperation operation);
        Task HandleSleeveChangeAsync(int id, CancellationToken cancellationToken = default);
    }
}
