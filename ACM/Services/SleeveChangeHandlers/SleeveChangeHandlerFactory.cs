using ACM.Services.SleeveChangeHandlers.Interfaces;
using Core.Common.Enums;

namespace ACM.Services.SleeveChangeHandlers
{
    public class SleeveChangeHandlerFactory : ISleeveChangeHandlerFactory
    {
        private readonly IEnumerable<ISleeveChangeHandler> _handlers;

        public SleeveChangeHandlerFactory(IEnumerable<ISleeveChangeHandler> handlers)
        {
            _handlers = handlers;
        }

        public ISleeveChangeHandler CreateHandler(CrudOperation operation)
        {
            return _handlers.FirstOrDefault(h => h.CanHandle(operation))
                ?? throw new ArgumentException($"Unsupported operation: {operation}", nameof(operation));
        }
    }
}
