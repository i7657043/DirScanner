using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;
using System.Text;

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
                       SetupLogger(options.Verbose);

                       Log.Logger.Debug("Process started");

                       if (!string.IsNullOrEmpty(options.Path))
                       {
                           commandLineOptions.Path = CapitaliseDriveLetterOfPath(options.Path);
                       }
                       else
                       {
                           string? path = GetPathFromInput();
                           if (path != null)
                               commandLineOptions.Path = path;
                           else
                               Log.Logger.Information($"Using default path: {commandLineOptions.Path} as the root of the search");
                       }
                       if (!Directory.Exists(commandLineOptions.Path))
                           throw new PathException(commandLineOptions.Path);

                       commandLineOptions.OutputSize = options.Size > 0 ? options.Size : commandLineOptions.OutputSize;

                   });

                   Log.Logger.Debug("Resource Lister Process starting...");

                   services.Configure((Action<ConsoleLifetimeOptions>)(options => options.SuppressStatusMessages = true));

                   services.AddSingleton(commandLineOptions)
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

    private static string CapitaliseDriveLetterOfPath(string path)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(char.ToUpper(path[0]));
        sb.Append(path.Substring(1));
        return sb.ToString();
    }

    private static string? GetPathFromInput()
    {
        Console.Write("Please enter a path to use as the root of the search (or hit enter to use the default): ");
        string path = Console.ReadLine() ?? string.Empty;
        if (!string.IsNullOrEmpty(path))
            return path;

        return null;
    }

    private static void SetupLogger(bool verboseLogging)
    {
        string loggerOutputFormat = verboseLogging ? "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}" : "{Message}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Is(verboseLogging ? Serilog.Events.LogEventLevel.Debug : Serilog.Events.LogEventLevel.Information)
        .WriteTo.File("log.txt", outputTemplate: loggerOutputFormat)
        .WriteTo.Console(outputTemplate: loggerOutputFormat)
        .CreateLogger();
    }
}
