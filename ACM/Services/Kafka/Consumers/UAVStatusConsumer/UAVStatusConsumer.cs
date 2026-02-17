using ACM.Models.Config;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace ACM.Services.Kafka.Consumers.UAVStatusConsumer
{
    public class UAVStatusConsumer : IUAVStatusConsumer
    {
        private readonly IConsumer<string, string> _kafkaConsumer;
        private readonly int _consumeTimeoutMs;
        private bool _isDisposed;

        public UAVStatusConsumer(IOptions<KafkaConfiguration> kafkaOptions)
        {
            KafkaConfiguration kafkaConfiguration = kafkaOptions.Value;
            ConsumerConfig config = new()
            {
                BootstrapServers = kafkaConfiguration.BootstrapServers,
                GroupId = kafkaConfiguration.GroupId,
                AutoOffsetReset = AutoOffsetReset.Latest,
            };

            _kafkaConsumer = new ConsumerBuilder<string, string>(config)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();
            _kafkaConsumer.Subscribe(kafkaConfiguration.StatusUpdateTopic);
            _consumeTimeoutMs = kafkaConfiguration.ConsumeTimeoutMs;
            _isDisposed = false;
        }

        public ConsumeResult<string, string> ConsumeUAVStatus()
        {
            if (_isDisposed)
                return null;
            try
            {
                return _kafkaConsumer.Consume(TimeSpan.FromMilliseconds(_consumeTimeoutMs));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            _kafkaConsumer.Unassign();
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
        }
    }
}
