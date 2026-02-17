using ACM.Models.Config;
using ACM.Services.Quartz.Jobs;
using ACM.Services.Quartz.Schedulers.UAVStatusScheduler.Interfaces;
using Microsoft.Extensions.Options;
using Quartz;

namespace ACM.Services.Quartz.Schedulers.UAVStatusScheduler
{
    public class UAVStatusScheduler : IUAVStatusScheduler
    {
        private readonly IScheduler _scheduler;
        private readonly QuartzConfiguration _quartzConfig;

        public UAVStatusScheduler(IScheduler scheduler, IOptions<QuartzConfiguration> quartzOptions)
        {
            _scheduler = scheduler;
            _quartzConfig = quartzOptions.Value;
        }

        public async Task StartScheduler(int intervalSeconds)
        {
            IJobDetail job = CreateJob();
            ITrigger trigger = CreateTrigger(intervalSeconds);

            await _scheduler.ScheduleJob(job, trigger);
            await _scheduler.Start();
        }

        public async Task StopScheduler()
        {
            JobKey jobKey = new(_quartzConfig.UAVStatusJobKey, _quartzConfig.UAVStatusGroupName);

            if (await _scheduler.CheckExists(jobKey))
            {
                await _scheduler.DeleteJob(jobKey);
            }

            if (_scheduler.IsStarted)
            {
                await _scheduler.Shutdown();
            }
        }

        private IJobDetail CreateJob()
        {
            return JobBuilder.Create<UAVStatusConsumerJob>()
                .WithIdentity(_quartzConfig.UAVStatusJobKey, _quartzConfig.UAVStatusGroupName)
                .Build();
        }

        private ITrigger CreateTrigger(int intervalSeconds)
        {
            return TriggerBuilder.Create()
                .WithIdentity(_quartzConfig.UAVStatusTriggerKey, _quartzConfig.UAVStatusGroupName)
                .StartNow()
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInSeconds(intervalSeconds)
                            .RepeatForever())
                .Build();
        }
    }
}
