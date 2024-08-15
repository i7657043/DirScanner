using FindBiggestResources.BLL;
using FindBiggestResources.Events;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

internal class ResourceLister : IResourceLister
{
    private readonly ILogger<IResourceLister> _logger;
    private readonly CommandLineOptions _commandLineOptions;
    private readonly IDirectoryScanner _directoryScanner;
    private readonly IProgressBar _progresssBar;
    private readonly IInputParsingHelpers _inputParsingHelpers;
    private readonly IProgressLogger _progressLogger;

    public event EventHandler<ProgressEventArgs> RecusringProgress;
    public event EventHandler<RecurseFinishedProgressEventArgs> RecurseFinishedProgress;
    public event EventHandler<EventArgs> AnalysingProgress;
    public event EventHandler<AnalysingFinishedProgressEventArgs> AnalysingFinishedProgress;

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

        RecusringProgress += _progressLogger.OnRecursing;

        RecurseFinishedProgress += async (sender, e) =>
            await _progressLogger.OnRecursedAsync(sender!, e);

        AnalysingProgress += _progressLogger.OnAnalysing;

        AnalysingFinishedProgress += async (sender, e) =>
            await _progressLogger.OnAnalysed(sender!, e);
    }

    public async Task<int> RunAsync()
    {
        (bool DrillDeeper, string Path) runInfo = (true, _commandLineOptions.Path);

        while (runInfo.DrillDeeper)
        {
            List<DirData> largestDirsListed = RecurseDirs(runInfo.Path);

            runInfo = _inputParsingHelpers.ParseNextPath(largestDirsListed, runInfo.Path);
        }

        return await Task.FromResult(0);
    }    

    private List<DirData> RecurseDirs(string path)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<DirData> dirs = new List<DirData>();
        
        RecusringProgress.Invoke(this, new ProgressEventArgs(path));
        _directoryScanner.RecurseDirsFromPath(path, dirs);
        dirs.RemoveAt(0);
        RecurseFinishedProgress.Invoke(this, new RecurseFinishedProgressEventArgs(dirs.Count));

        AnalysingProgress.Invoke(this, new EventArgs());
        List<DirData> analysedDirs = _directoryScanner.GetAllDirSizes(dirs).FilterTopMatchesBySize();
        AnalysingFinishedProgress.Invoke(this,
            new AnalysingFinishedProgressEventArgs(analysedDirs, _commandLineOptions.OutputSize, sw.ElapsedMilliseconds / 1000));

        return analysedDirs;
    }

    private void LogDirBySize(List<DirData> matches, int numOfFilesToShowInOutput)
    {
        int counter = 0;
        _logger.LogInformation($"{"#",-3} {"Path",-60} {"Size"}");

        foreach (DirData dir in matches.Take(numOfFilesToShowInOutput))
        {
            double sizeMB = dir.Size / 1024;
            double sizeGB = sizeMB / 1024;
            string dirSize = (sizeMB > 1024) ? $"{sizeGB.ToTwoDecimalPlaces()}GB" : ((dir.Size > 1024) ? $"{sizeMB.ToTwoDecimalPlaces()}MB" : $"{dir.Size.ToTwoDecimalPlaces()}KB");

            _logger.LogInformation($"{++counter,-3} {dir.Path,-60} {dirSize}");
        }
    }
}
