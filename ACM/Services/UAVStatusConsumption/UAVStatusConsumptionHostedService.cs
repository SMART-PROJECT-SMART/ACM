using ACM.Models.Dto;
using ACM.Services.AssignmentUpdate.Interfaces;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using ACM.Services.UAVStatusConsumption.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ACM.Services.UAVStatusConsumption
{
    public class UAVStatusConsumptionHostedService : IUAVStatusConsumptionHostedService
    {
        private readonly IAssignmentUpdateService _assignmentUpdateService;
        private readonly IUAVStatusConsumer _uavStatusConsumer;
        private readonly ILogger<UAVStatusConsumptionHostedService> _logger;
        private Task? _runTask;
        private CancellationTokenSource? _cts;

        public UAVStatusConsumptionHostedService(
            IAssignmentUpdateService assignmentUpdateService,
            IUAVStatusConsumer uavStatusConsumer,
            ILogger<UAVStatusConsumptionHostedService> logger)
        {
            _assignmentUpdateService = assignmentUpdateService;
            _uavStatusConsumer = uavStatusConsumer;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runTask = RunAsync(_cts.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_cts == null)
            {
                return;
            }

            await _cts.CancelAsync();
            if (_runTask != null)
            {
                await _runTask;
            }
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    IEnumerable<UAVStatusData> statusData = await Task.Run(
                        () => _uavStatusConsumer.ConsumeUAVStatus(stoppingToken),
                        stoppingToken);
                    List<UAVStatusData> statusList = statusData.ToList();

                    if (statusList.Count == 0)
                    {
                        continue;
                    }

                    await _assignmentUpdateService.RunWithStatusAsync(
                        statusList,
                        stoppingToken
                    );
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UAV status consumption cycle failed");
                }
            }
        }
    }
}
