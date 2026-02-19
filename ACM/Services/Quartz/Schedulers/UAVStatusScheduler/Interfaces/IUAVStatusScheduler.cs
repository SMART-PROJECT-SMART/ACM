namespace ACM.Services.Quartz.Schedulers.UAVStatusScheduler.Interfaces
{
    public interface IUAVStatusScheduler : IHostedService
    {
        Task StartScheduler(int intervalSeconds);
        Task StopScheduler();
    }
}
