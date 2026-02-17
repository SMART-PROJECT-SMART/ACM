using Core.Common.Enums;

namespace ACM.Services.SleeveChangeHandlers.Interfaces
{
    public interface ISleeveChangeHandlerFactory
    {
        ISleeveChangeHandler CreateHandler(CrudOperation operation);
    }
}
