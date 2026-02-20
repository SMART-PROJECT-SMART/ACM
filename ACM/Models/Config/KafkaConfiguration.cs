namespace ACM.Models.Config
{
    public class KafkaConfiguration
    {
        public string BootstrapServers { get; set; }
        public string StatusUpdateTopic { get; set; }
        public string GroupId { get; set; }
        public int ConsumeTimeoutMs { get; set; }
        public int MaxPollIntervalMs { get; set; }
        public int AutoCommitIntervalMs { get; set; }
    }
}
