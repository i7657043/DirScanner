using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

internal class ConsoleHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IDirScannerController _app;
    
    public ConsoleHostedService(ILogger<ConsoleHostedService> logger, IHostApplicationLifetime appLifetime, IDirScannerController app)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _app = app;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);

                string? appName = Assembly.GetExecutingAssembly().GetName().Name;

                try
                {
                    int exitCode = await _app.RunAsync();

                    _logger.LogDebug("{AppName} Process finished with Exit Code: {@ExitCode}", appName, exitCode);

                    _appLifetime.StopApplication();
                }                
                catch (PathException pex)
                {
                    _logger.LogCritical("{AppName} Process exited early with path exception {@Exception}", appName, pex);

                    Environment.Exit(-2);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("{AppName} Process exited early with exception {@Exception}", appName, ex);

                    Environment.Exit(-1);
                }
            });
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

