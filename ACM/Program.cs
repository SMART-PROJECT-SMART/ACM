using ACM.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWebApi()
    .AddAppConfiguration(builder.Configuration)
    .AddSleeveServices()
    .AddDeviceManagerClient(builder.Configuration);

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
