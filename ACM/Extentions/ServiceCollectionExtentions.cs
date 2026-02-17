using ACM.Common;
using ACM.Models.Config;
using ACM.Services.DeviceManagerClient;
using ACM.Services.DeviceManagerClient.Interfaces;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using ACM.Services.Kafka.Consumers.UAVStatusConsumer;
using ACM.Services.SleeveChangeHandlers;
using ACM.Services.SleeveChangeHandlers.Handlers;
using ACM.Services.SleeveChangeHandlers.Interfaces;
using ACM.Services.SleeveManager;
using ACM.Services.SleeveManager.Interfaces;
using ACM.Services.StartUpSleeveFetcher;
using ACM.Services.StartUpSleeveFetcher.Interfaces;
using Quartz;

namespace ACM.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            return services;
        }

        public static IServiceCollection AddAppConfiguration(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<KafkaConfiguration>(
                configuration.GetSection(ACMConstants.Configuration.KAFKA_CONFIG_SECTION)
            );
            services.Configure<DeviceManagerConfiguration>(
                configuration.GetSection(ACMConstants.Configuration.DEVICE_MANAGER_CONFIG_SECTION)
            );
            services.Configure<QuartzConfiguration>(
                configuration.GetSection(ACMConstants.Configuration.QUARTZ_CONFIG_SECTION)
            );
            return services;
        }

        public static IServiceCollection AddSleeveServices(this IServiceCollection services)
        {
            services.AddSingleton<ISleeveManager, SleeveManager>();
            services.AddScoped<ISleeveChangeHandler, SleeveCreatedHandler>();
            services.AddScoped<ISleeveChangeHandler, SleeveUpdatedHandler>();
            services.AddScoped<ISleeveChangeHandler, SleeveDeletedHandler>();
            services.AddScoped<ISleeveChangeHandlerFactory, SleeveChangeHandlerFactory>();
            services.AddHostedService<StartUpSleeveFetcher>();
            return services;
        }

        public static IServiceCollection AddKafkaServices(this IServiceCollection services)
        {
            services.AddSingleton<IUAVStatusConsumer, UAVStatusConsumer>();
            return services;
        }

        public static IServiceCollection AddDeviceManagerClient(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            DeviceManagerConfiguration config = configuration
                .GetSection(ACMConstants.Configuration.DEVICE_MANAGER_CONFIG_SECTION)
                .Get<DeviceManagerConfiguration>()!;

            services.AddHttpClient(
                ACMConstants.HttpClients.DEVICE_MANAGER_HTTP_CLIENT,
                client =>
                {
                    client.BaseAddress = new Uri(config.BaseUrl);
                }
            );

            services.AddScoped<IDeviceManagerClient, DeviceManagerClient>();
            return services;
        }

        public static IServiceCollection AddQuartzServices(this IServiceCollection services)
        {
            services.AddQuartz();
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            services.AddSingleton(provider =>
                provider
                    .GetRequiredService<ISchedulerFactory>()
                    .GetScheduler()
                    .GetAwaiter()
                    .GetResult()
            );
            return services;
        }
    }
}
