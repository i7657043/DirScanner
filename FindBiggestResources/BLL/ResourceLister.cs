using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

internal class ResourceLister : IResourceLister
{
    private readonly ILogger<IResourceLister> _logger;
    private readonly CommandLineOptions _commandLineOptions;
    private readonly IDirectoryScanner _directoryScanner;

    public ResourceLister(ILogger<IResourceLister> logger, CommandLineOptions commandLineOptions, IDirectoryScanner directoryScanner)
    {
        _logger = logger;
        _commandLineOptions = commandLineOptions;
        _directoryScanner = directoryScanner;
    }

    public async Task<int> RunAsync()
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<DirData> dirs = new List<DirData>();

        _logger.LogInformation($"Recursing all directories from {_commandLineOptions.Path}");

        Task processingTask = ConoleLoggingOutput.ShowWaitOutputAsync();
        _directoryScanner.RecurseDirsFromPath(_commandLineOptions.Path, dirs);
        dirs.RemoveAt(0);
        ConoleLoggingOutput.IsProcessing = false;
        await processingTask;

        _logger.LogInformation("\nDone. Found {NoOfDirs} directories to be analysed. Scanning for largest directories", dirs.Count);

        processingTask = ConoleLoggingOutput.ShowWaitOutputAsync();
        List<DirData> analysedDirs = GetAllDirSizes(dirs);
        ConoleLoggingOutput.IsProcessing = false;
        await processingTask;

        LogDirBySize(analysedDirs.FilterTopMatchesBySize(), _commandLineOptions.OutputSize);

        _logger.LogInformation("All operations complete in {Time} sec/s", sw.ElapsedMilliseconds / 1000);

        return await Task.FromResult(0);
    }

    private static List<DirData> GetAllDirSizes(List<DirData> dirs)
    {
        ConcurrentBag<DirData> concurrentMatches = new ConcurrentBag<DirData>();

        Parallel.ForEach(dirs.DirsByDeep(), dir =>
        {
            lock (concurrentMatches)
            {
                if (concurrentMatches.Any(d => dir.Path.Contains(d.Path)))
                    return;
            }

            concurrentMatches.Add(new DirData(
                dir.Path, 
                dirs.Where(d => d.Path.Contains(dir.Path)).ToList().GetTotalSizeInKb()));
        });

        return concurrentMatches.ToList();
    }

    private void LogDirBySize(List<DirData> matches, int numOfFilesToShowInOutput)
    {
        int counter = 0;
        _logger.LogInformation($"\n{"#",-3} {"Path",-60} {"Size"}");

        foreach (DirData dir in matches.Take(numOfFilesToShowInOutput))
        {
            double sizeMB = dir.Size / 1024;
            double sizeGB = sizeMB / 1024;
            string dirSize = (sizeMB > 1024) ? $"{sizeGB.ToTwoDecimalPlaces()}GB" : ((dir.Size > 1024) ? $"{sizeMB.ToTwoDecimalPlaces()}MB" : $"{dir.Size.ToTwoDecimalPlaces()}KB");

            _logger.LogInformation($"{++counter,-3} {dir.Path,-60} {dirSize}");
        }
    }
}
