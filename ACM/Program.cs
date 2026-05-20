using ACM.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddWebApi()
    .AddAppConfiguration(builder.Configuration)
    .AddSleeveServices()
    .AddDeviceManagerClient(builder.Configuration)
    .AddSimulatorClient(builder.Configuration)
    .AddKafkaServices()
    .AddAssignmentServices()
    .AddUAVStatusConsumption();

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();
