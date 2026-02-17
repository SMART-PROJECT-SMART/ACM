using ACM.Models.Dto;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using Quartz;

namespace ACM.Services.Quartz.Jobs
{
    public class UAVStatusConsumerJob : IJob
    {
        private readonly IUAVStatusConsumer _uavStatusConsumer;

        public UAVStatusConsumerJob(IUAVStatusConsumer uavStatusConsumer)
        {
            _uavStatusConsumer = uavStatusConsumer;
        }

        public Task Execute(IJobExecutionContext context)
        {
            IEnumerable<UAVStatusData> uavsStatusData = _uavStatusConsumer.ConsumeUAVStatus(
                context.CancellationToken
            );
            return Task.CompletedTask;
        }
    }
}
