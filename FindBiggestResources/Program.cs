using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Reflection;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await Host.CreateDefaultBuilder(args)
               .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
               .ConfigureAppConfiguration((hostContext, builder) =>
               {
                   builder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();
               })
               .ConfigureServices((hostContext, services) =>
               {
                   string loggerOutputFormat = "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

                   SetupLogger(loggerOutputFormat);

                   Log.Logger.Information("Resource Lister Process starting...");

                   services.AddSingleton<IResourceLister, ResourceLister>().AddHostedService<ConsoleHostedService>();
               })
               .UseSerilog()
               .RunConsoleAsync();
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal("There was a fatal error on Startup. Exiting application {@Exception}", ex);
        }
    }

    private static void SetupLogger(string loggerOutputFormat)
    {
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File("log.txt", outputTemplate: loggerOutputFormat)
        .WriteTo.Console(outputTemplate: loggerOutputFormat)
        .CreateLogger();
    }
}