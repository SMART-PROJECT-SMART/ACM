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
        private readonly CancellationTokenSource _disposeCts;

        public UAVStatusConsumer(IOptions<KafkaConfiguration> kafkaOptions)
        {
            KafkaConfiguration kafkaConfiguration = kafkaOptions.Value;
            ConsumerConfig config = new()
            {
                BootstrapServers = kafkaConfiguration.BootstrapServers,
                GroupId = kafkaConfiguration.GroupId,
                AutoOffsetReset = AutoOffsetReset.Latest,
            };
            _disposeCts = new CancellationTokenSource();

            _kafkaConsumer = new ConsumerBuilder<string, string>(config)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();
            _kafkaConsumer.Subscribe(kafkaConfiguration.StatusUpdateTopic);
            _consumeTimeoutMs = kafkaConfiguration.ConsumeTimeoutMs;
        }

        public IEnumerable<UAVStatusData> ConsumeUAVStatus(
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                using CancellationTokenSource linkedCts =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        _disposeCts.Token,
                        cancellationToken
                    );
                linkedCts.CancelAfter(_consumeTimeoutMs);

                ConsumeResult<string, string> uavsStatus = _kafkaConsumer.Consume(linkedCts.Token);

                if (uavsStatus?.Message?.Value is null)
                    return [];

                UAVStatusData? statusData = JsonConvert.DeserializeObject<UAVStatusData>(
                    uavsStatus.Message.Value
                );

                return statusData is not null ? [statusData] : [];
            }
            catch (OperationCanceledException)
            {
                return [];
            }
        }

        public void Dispose()
        {
            _disposeCts.Cancel();
            _disposeCts.Dispose();
            _kafkaConsumer.Unassign();
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
        }
    }
}
