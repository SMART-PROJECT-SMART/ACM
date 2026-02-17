namespace ACM.Models.Config
{
    public class QuartzConfiguration
    {
        public int UAVStatusJobInterval { get; set; }
        public string UAVStatusJobKey { get; set; }
        public string UAVStatusGroupName { get; set; }
        public string UAVStatusTriggerKey { get; set; }
    }
}
