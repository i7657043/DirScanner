using DirScanner.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

internal class DirScannerController : IDirScannerController
{
    private readonly ILogger<IDirScannerController> _logger;
    private readonly CommandLineOptions _commandLineOptions;
    private readonly IDirectoryScanner _directoryScanner;
    private readonly IProgressBar _progresssBar;
    private readonly IInputParsingHelpers _inputParsingHelpers;
    private readonly IProgressLogger _progressLogger;

    public DirScannerController(
        ILogger<IDirScannerController> logger, 
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
        (bool DrillDeeper, string Path) directoryScanningOptions = (true, _commandLineOptions.Path);

        while (directoryScanningOptions.DrillDeeper)
            directoryScanningOptions = _inputParsingHelpers.GetUserInputForWhichPathToDrillInto(
                await RecurseDirs(directoryScanningOptions.Path), 
                directoryScanningOptions.Path);        

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
        dirs = _directoryScanner.GetAllDirSizes(dirs).FilterTopMatchesBySize();
        await _progressLogger.LogAnalysed(dirs, _commandLineOptions.OutputSize, sw.ElapsedMilliseconds / 1000);

        return dirs;
    }
}
