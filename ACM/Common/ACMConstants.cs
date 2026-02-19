namespace ACM.Common
{
    public static class ACMConstants
    {
        public static class Configuration
        {
            public const string DEVICE_MANAGER_CONFIG_SECTION = "DeviceManager";
            public const string KAFKA_CONFIG_SECTION = "Kafka";
            public const string QUARTZ_CONFIG_SECTION = "Quartz";
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
        }

        public static class SimulationApiEndpoints
        {
            public const string UAV_PORTS_CHANGED = "api/DeviceManagerWebhook/uav-ports-changed";
        }

        public static class Scoring
        {
            public const double MIN_DISTANCE_DENOMINATOR = 1.0;
        }
    }
}
