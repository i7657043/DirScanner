using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

internal class ConsoleHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IResourceLister _app;
    
    public ConsoleHostedService(ILogger<ConsoleHostedService> logger, IHostApplicationLifetime appLifetime, IResourceLister app)
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
                    _logger.LogInformation("{AppName} Process started", appName);

                    int exitCode = await _app.Run();

                    _logger.LogInformation("{AppName} Process finished with Exit Code: {@ExitCode}", appName, exitCode);

                    _appLifetime.StopApplication();
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

