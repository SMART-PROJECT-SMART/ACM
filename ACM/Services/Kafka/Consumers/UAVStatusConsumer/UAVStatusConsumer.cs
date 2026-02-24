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
        private readonly int _pollTimeoutMs;
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
            _pollTimeoutMs = kafkaConfiguration.ConsumeTimeoutMs;
        }

        public IEnumerable<UAVStatusData> ConsumeUAVStatus(
            CancellationToken cancellationToken = default
        )
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return [];
            }

            try
            {
                ConsumeResult<string, string>? consumeResult =
                    _kafkaConsumer.Consume(TimeSpan.FromMilliseconds(_pollTimeoutMs));

                if (consumeResult?.Message?.Value is null)
                    return [];

                string value = consumeResult.Message.Value;
                JsonSerializerSettings jsonSettings = new()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                };
                string trimmed = value.TrimStart();
                if (trimmed.StartsWith("["))
                {
                    List<UAVStatusData>? statusList =
                        JsonConvert.DeserializeObject<List<UAVStatusData>>(value, jsonSettings);
                    if (statusList is null || statusList.Count == 0)
                        return [];

                    return statusList;
                }

                UAVStatusData? single =
                    JsonConvert.DeserializeObject<UAVStatusData>(value, jsonSettings);
                if (single is null)
                    return [];

                return [single];
            }
            catch (OperationCanceledException)
            {
                return [];
            }
            catch (ConsumeException)
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
