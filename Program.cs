using NetCoreAPIRainfall;
using NLog.Web;
using NLog;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Info("Starting the Rainfall Api.");

    var builder = WebApplication.CreateBuilder(args);

    var startup = new Startup(builder.Configuration);
    startup.ConfigureServices(builder.Services);
    startup.ConfigureLogging(builder.Logging);
    builder.Host.UseNLog();

    var webApplication = builder.Build();

    startup.ConfigureAppConfiguration(builder);
    startup.Configure(webApplication, builder.Environment);

    webApplication.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped Rainfall Api program because of a start-up exception.");
    throw;
}
finally
{
    LogManager.Shutdown();
}