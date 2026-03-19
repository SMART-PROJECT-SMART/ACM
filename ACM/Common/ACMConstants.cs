namespace ACM.Common
{
    public static class ACMConstants
    {
        public static class Configuration
        {
            public const string DEVICE_MANAGER_CONFIG_SECTION = "DeviceManager";
            public const string KAFKA_CONFIG_SECTION = "Kafka";
            public const string SIMULATION_CONFIG_SECTION = "Simulation";
        }

        public static class HttpClients
        {
            public const string DEVICE_MANAGER_HTTP_CLIENT = "DeviceManagerHttpClient";
            public const string SIMULATOR_HTTP_CLIENT = "SimulatorHttpClient";
        }

        public static class DeviceManagerApiEndpoints
        {
            public const string GET_ALL_SLEEVES = "api/sleeve";
            public const string GET_SLEEVE_BY_NAME = "api/sleeve/{0}";
            public const string ASSIGN_SLEEVE_TO_UAV = "api/sleeve/assign";
            public const string RELEASE_SLEEVE_BY_TAIL_ID = "api/sleeve/release/{0}";
        }

        public static class SimulationApiEndpoints
        {
            public const string UAV_PORTS_CHANGED = "api/DeviceManagerWebhook/uav-ports-changed";
        }

        public static class Assignment
        {
            public const int DUMMY_COST = 1_000_000_000;
            public const double COST_SCALE_FACTOR = 100_000;
        }
    }
}
