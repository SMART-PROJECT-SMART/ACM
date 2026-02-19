using ACM.Models.Config;
using ACM.Models.Dto;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ACM.Services.Kafka.Consumers.UAVStatusConsumer
{
    public class UAVStatusConsumer : IUAVStatusConsumer
    {
        private readonly IConsumer<string, string> _kafkaConsumer;
        private readonly int _consumeTimeoutMs;
        private readonly CancellationTokenSource _disposeCts;
        private readonly ILogger<UAVStatusConsumer> _logger;

        public UAVStatusConsumer(
            IOptions<KafkaConfiguration> kafkaOptions,
            ILogger<UAVStatusConsumer> logger)
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
            _logger = logger;
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
                {
                    _logger.LogDebug("Kafka consume: no message (timeout or empty)");
                    return [];
                }

                string value = uavsStatus.Message.Value;
                List<UAVStatusData>? statusList = JsonConvert.DeserializeObject<List<UAVStatusData>>(value);
                if (statusList is null || statusList.Count == 0)
                {
                    UAVStatusData? single = JsonConvert.DeserializeObject<UAVStatusData>(value);
                    if (single is null)
                    {
                        _logger.LogWarning("Kafka consume: message could not be deserialized to UAVStatusData or array");
                        return [];
                    }
                    _logger.LogDebug("Kafka consume: received status for TailId {TailId}", single.TailId);
                    return [single];
                }

                _logger.LogDebug("Kafka consume: received status for {Count} UAVs", statusList.Count);
                return statusList;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Kafka consume: cancelled (timeout or token)");
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
