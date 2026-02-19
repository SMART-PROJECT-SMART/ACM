using ACM.Common;
using ACM.Models.Config;
using ACM.Services.AssignmentManager;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.AssignmentUpdate;
using ACM.Services.AssignmentUpdate.Interfaces;
using ACM.Services.DeviceManagerClient;
using ACM.Services.DeviceManagerClient.Interfaces;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using ACM.Services.Kafka.Consumers.UAVStatusConsumer;
using ACM.Services.ScoreCalculator;
using ACM.Services.ScoreCalculator.Interfaces;
using ACM.Services.SimulatorClient;
using ACM.Services.SimulatorClient.Interfaces;
using ACM.Services.Quartz.Schedulers.UAVStatusScheduler;
using ACM.Services.Quartz.Schedulers.UAVStatusScheduler.Interfaces;
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
            services.Configure<SimulationConfiguration>(
                configuration.GetSection(ACMConstants.Configuration.SIMULATION_CONFIG_SECTION)
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

            services.AddHttpClient<IDeviceManagerClient, DeviceManagerClient>(client =>
            {
                client.BaseAddress = new Uri(config.BaseUrl);
            });
            return services;
        }

        public static IServiceCollection AddSimulatorClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            SimulationConfiguration config = configuration
                .GetSection(ACMConstants.Configuration.SIMULATION_CONFIG_SECTION)
                .Get<SimulationConfiguration>()!;

            services.AddHttpClient<ISimulatorClient, SimulatorClient>(client =>
            {
                client.BaseAddress = new Uri(config.BaseUrl);
            });

            return services;
        }

        public static IServiceCollection AddAssignmentServices(this IServiceCollection services)
        {
            services.AddSingleton<IScoreCalculator, DistanceScoreCalculator>();
            services.AddSingleton<IAssignmentManager, AssignmentManager>();
            services.AddScoped<IAssignmentUpdateService, AssignmentUpdateService>();
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
            services.AddSingleton<IUAVStatusScheduler, UAVStatusScheduler>();
            services.AddHostedService(provider => provider.GetRequiredService<IUAVStatusScheduler>());
            return services;
        }
    }
}
