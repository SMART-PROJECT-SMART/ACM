using ACM.Services.AssignmentUpdate.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ACM.Services.Quartz.Jobs
{
    public class UAVStatusConsumerJob : IJob
    {
        private readonly IAssignmentUpdateService _assignmentUpdateService;
        private readonly ILogger<UAVStatusConsumerJob> _logger;

        public UAVStatusConsumerJob(
            IAssignmentUpdateService assignmentUpdateService,
            ILogger<UAVStatusConsumerJob> logger)
        {
            _assignmentUpdateService = assignmentUpdateService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("UAV status consumer job started");
            await _assignmentUpdateService.RunAsync(context.CancellationToken);
            _logger.LogInformation("UAV status consumer job completed");
        }
    }
}
