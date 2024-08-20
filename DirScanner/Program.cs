using CommandLine;
using DirScanner.Extensions;
using DirScanner.Services.Abstractions;
using DirScanner.Services.Implementations;
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
                       Log.Logger = new LoggerConfiguration()
                       .MinimumLevel.Is(options.Verbose ? Serilog.Events.LogEventLevel.Verbose : Serilog.Events.LogEventLevel.Information)
                       .WriteTo.Console(outputTemplate: options.Verbose ? "{Timestamp:HH:mm:ss} {Message}{NewLine}{Exception}" : "{Message}{NewLine}{Exception}")
                       .CreateLogger();

                       commandLineOptions = options.ParseCommandLineOptions();
                   });

                   services.Configure((Action<ConsoleLifetimeOptions>)(options => options.SuppressStatusMessages = true));

                   services.AddSingleton(commandLineOptions)
                   .AddSingleton<IProgressLogger, ProgressLogger>()
                   .AddSingleton<IInputParsingHelpers, InputParsingHelpers>()
                   .AddSingleton<IProgressBar, ProgressBar>()
                   .AddSingleton<IDirectoryScanner, DirectoryScanner>()
                   .AddSingleton<IDirScannerController, DirScannerController>()
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
}
