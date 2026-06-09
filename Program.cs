using APITesting.Common.Constants;
using APITesting.Common.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder
    .AddApplicationConfiguration()
    .AddApplicationLogging();

try
{
    builder.Configuration.ValidateApplicationConfiguration();
    builder.Services
        .AddApplicationControllers()
        .AddApplicationSwagger(builder.Configuration)
        .AddApplicationDependencies(builder.Configuration);

    var app = builder.Build();
    app.UseApplicationSwagger();
    app.UseApplicationPipeline();

    Log.Information(GlobalParams.Logs.AppStarting);
    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, GlobalParams.Logs.AppStartFailed);
}
finally
{
    Log.CloseAndFlush();
}
