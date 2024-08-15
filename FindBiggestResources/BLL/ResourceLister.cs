using FindBiggestResources.BLL;
using FindBiggestResources.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

internal class ResourceLister : IResourceLister
{
    private readonly ILogger<IResourceLister> _logger;
    private readonly CommandLineOptions _commandLineOptions;
    private readonly IDirectoryScanner _directoryScanner;
    private readonly IProgressBar _progresssBar;
    private readonly IInputParsingHelpers _inputParsingHelpers;
    private readonly IProgressLogger _progressLogger;

    public ResourceLister(
        ILogger<IResourceLister> logger, 
        CommandLineOptions commandLineOptions, 
        IDirectoryScanner directoryScanner,
        IProgressBar progressBar,
        IInputParsingHelpers inputParsingHelpers,
        IProgressLogger progressLogger)
    {
        _logger = logger;
        _commandLineOptions = commandLineOptions;
        _directoryScanner = directoryScanner;
        _progresssBar = progressBar;
        _inputParsingHelpers = inputParsingHelpers;
        _progressLogger = progressLogger;
    }

    public async Task<int> RunAsync()
    {
        (bool DrillDeeper, string Path) runInfo = (true, _commandLineOptions.Path);

        while (runInfo.DrillDeeper)
        {
            List<DirData> largestDirsListed = await RecurseDirs(runInfo.Path);

            runInfo = _inputParsingHelpers.ParseNextPath(largestDirsListed, runInfo.Path);
        }

        return await Task.FromResult(0);
    }    

    private async Task<List<DirData>> RecurseDirs(string path)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<DirData> dirs = new List<DirData>();

        _progressLogger.LogRecursing(path);
        _directoryScanner.RecurseDirsFromPath(path, dirs);
        dirs.RemoveAt(0);
        await _progressLogger.LogRecursedAsync(dirs.Count);

        _progressLogger.LogAnalysing();
        List<DirData> analysedDirs = _directoryScanner.GetAllDirSizes(dirs).FilterTopMatchesBySize();
        await _progressLogger.LogAnalysed(analysedDirs, _commandLineOptions.OutputSize, sw.ElapsedMilliseconds / 1000);

        return analysedDirs;
    }
}
