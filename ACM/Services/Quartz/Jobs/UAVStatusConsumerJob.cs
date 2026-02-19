using ACM.Services.AssignmentUpdate.Interfaces;
using Quartz;

namespace ACM.Services.Quartz.Jobs
{
    public class UAVStatusConsumerJob : IJob
    {
        private readonly IAssignmentUpdateService _assignmentUpdateService;

        public UAVStatusConsumerJob(IAssignmentUpdateService assignmentUpdateService)
        {
            _assignmentUpdateService = assignmentUpdateService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _assignmentUpdateService.RunAsync(context.CancellationToken);
        }
    }
}
