using CommandLine;
using FindBiggestResources.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
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
                   CommandLineOptions commandLineOptions = new CommandLineOptions();

                   Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(options =>
                   {
                       Log.Logger = SetupLogger(options.Verbose);

                       commandLineOptions = options.ParseCommandLineOptions();
                   });

                   Log.Logger.Debug("Resource Lister Process starting...");

                   services.Configure((Action<ConsoleLifetimeOptions>)(options => options.SuppressStatusMessages = true));

                   services.AddSingleton(commandLineOptions)
                   .AddSingleton<IProgressBar, ProgressBar>()
                   .AddSingleton<IDirectoryScanner, DirectoryScanner>()
                   .AddSingleton<IResourceLister, ResourceLister>()
                   .AddHostedService<ConsoleHostedService>();
               })
               .UseSerilog()
               .RunConsoleAsync();
        }
        catch (ArgumentException aex)
        {
            Log.Logger.Fatal($"{aex.Message}");
        }
        catch (PathException pex)
        {
            Log.Logger.Fatal("Path: {Path} was not recognised. Please choose another", pex.Path);
        }        
        catch (Exception ex)
        {
            Log.Logger.Fatal("There was a fatal error on Startup. Exiting application {@Exception}", ex);
        }
    }

    private static ILogger SetupLogger(bool verboseLogging)
    {
        return new LoggerConfiguration()
        .MinimumLevel.Is(verboseLogging ? Serilog.Events.LogEventLevel.Debug : Serilog.Events.LogEventLevel.Information)
        .WriteTo.Console(outputTemplate: verboseLogging ? "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}" : "{Message}{NewLine}{Exception}")
        .CreateLogger();
    }
}
