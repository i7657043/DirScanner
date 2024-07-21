using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                try
                {
                    _logger.LogInformation("Process started");

                    int exitCode = await _app.Run();

                    _logger.LogInformation("Process finished with Exit Code: {@ExitCode}", exitCode);

                    _appLifetime.StopApplication();
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("Process exited early with exception {@Exception}", ex);

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

