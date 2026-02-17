namespace ACM.Common
{
    public static class ACMConstants
    {
        public static class Configuration
        {
            public const string DEVICE_MANAGER_CONFIG_SECTION = "DeviceManager";
        }

        public static class HttpClients
        {
            public const string DEVICE_MANAGER_HTTP_CLIENT = "DeviceManagerHttpClient";
        }

        public static class DeviceManagerApiEndpoints
        {
            public const string GET_ALL_SLEEVES = "api/sleeve";
            public const string GET_SLEEVE_BY_NAME = "api/sleeve/{0}";
        }
    }
}
