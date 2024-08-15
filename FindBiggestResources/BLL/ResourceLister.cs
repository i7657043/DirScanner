using FindBiggestResources.BLL;
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

    public ResourceLister(
        ILogger<IResourceLister> logger, 
        CommandLineOptions commandLineOptions, 
        IDirectoryScanner directoryScanner,
        IProgressBar progressBar,
        IInputParsingHelpers inputParsingHelpers)
    {
        _logger = logger;
        _commandLineOptions = commandLineOptions;
        _directoryScanner = directoryScanner;
        _progresssBar = progressBar;
        _inputParsingHelpers = inputParsingHelpers;
    }

    public async Task<int> RunAsync()
    {
        (bool DrillDeeper, string Path) runInfo = (true, _commandLineOptions.Path);

        while (runInfo.DrillDeeper)
        {
            List<DirData> largestDirsListed = await RecurseDirsAsync(runInfo.Path);

            runInfo = _inputParsingHelpers.ParseNextPath(largestDirsListed, runInfo.Path);
        }

        return await Task.FromResult(0);
    }    

    private async Task<List<DirData>> RecurseDirsAsync(string path)
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<DirData> dirs = new List<DirData>();

        _logger.LogInformation($"Recursing all directories from {path}");

        _progresssBar.Start();
        _directoryScanner.RecurseDirsFromPath(path, dirs);
        dirs.RemoveAt(0);
        await _progresssBar.Stop();

        _logger.LogInformation("Done. Found {NoOfDirs} directories to be analysed. Scanning for largest directories", dirs.Count);

        _progresssBar.Start();
        List<DirData> analysedDirs = _directoryScanner.GetAllDirSizes(dirs).FilterTopMatchesBySize();
        await _progresssBar.Stop();

        LogDirBySize(analysedDirs, _commandLineOptions.OutputSize);

        _logger.LogInformation("All operations complete in {Time} sec/s", sw.ElapsedMilliseconds / 1000);

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
