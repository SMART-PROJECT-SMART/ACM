using ACM.Models.Config;
using ACM.Models.Dto;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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

        public IEnumerable<UAVStatusData> ConsumeUAVStatus(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                return [];
            try
            {
                using CancellationTokenSource timeoutCts = new(TimeSpan.FromMilliseconds(_consumeTimeoutMs));
                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, timeoutCts.Token
                );

                ConsumeResult<string, string> uavsStatus = _kafkaConsumer.Consume(linkedCts.Token);

                if (uavsStatus?.Message?.Value is null)
                    return [];

                UAVStatusData? statusData = JsonConvert.DeserializeObject<UAVStatusData>(uavsStatus.Message.Value);

                return statusData is not null ? [statusData] : [];
            }
            catch (OperationCanceledException)
            {
                return [];
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
