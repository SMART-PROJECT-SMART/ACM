using Confluent.Kafka;

namespace ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces
{
    public interface IUAVStatusConsumer : IDisposable
    {
        public ConsumeResult<string, string> ConsumeUAVStatus();
    }
}
