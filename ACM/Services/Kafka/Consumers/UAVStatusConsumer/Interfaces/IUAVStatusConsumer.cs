using ACM.Models.Dto;

namespace ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces
{
    public interface IUAVStatusConsumer : IDisposable
    {
        IEnumerable<UAVStatusData> ConsumeUAVStatus(CancellationToken cancellationToken = default);
    }
}
