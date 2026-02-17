namespace ACM.Services.Quartz.Schedulers.UAVStatusScheduler.Interfaces
{
    public interface IUAVStatusScheduler
    {
        Task StartScheduler(int intervalSeconds);
        Task StopScheduler();
    }
}
