using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

internal class ResourceLister : IResourceLister
{
    private readonly ILogger<IResourceLister> _logger;
    private int _persistentCounter;

    public ResourceLister(ILogger<IResourceLister> logger)
    {
        _logger = logger;
    }

    public async Task<int> Run()
    {
        Stopwatch sw = new Stopwatch();
        List<DirData> dirs = new List<DirData>();

        //inputs
        string startPath = @"C:\";
        int searchLevelsDeep = 4;

        _logger.LogDebug("Recursing all directories...");

        sw.Start();

        RecurseDirsFromPath(startPath, dirs);

        dirs.RemoveAt(0);

        List<DirData> matches = new List<DirData>();

        _logger.LogDebug("Finding largest directories...");

        _persistentCounter = 0;

        List<DirData> filteredDirs = dirs.GetDirsDeep(searchLevelsDeep).FilterWindowsDirectories();

        _logger.LogDebug("Done. Found {NoOfDirs} directories in {Time} sec/s", dirs.Count, sw.ElapsedMilliseconds / 1000);
        _logger.LogDebug("Filtered down to {FilteredDirsCount} directories", filteredDirs.Count);

        Parallel.ForEach(filteredDirs, dirAlreadyScanned =>
        {
            //if (_persistentCounter > 0 && _persistentCounter % 500 == 0)
            //    _logger.LogDebug($"Analysed size of {_persistentCounter}/{filteredDirs.Count} directories");

            matches.Add(new DirData(dirAlreadyScanned.Path, GetTotalSizeInKb(filteredDirs, dirAlreadyScanned)));
        });

        //foreach (DirData dirAlreadyScanned in filteredDirs)
        //{
        //    if (_persistentCounter > 0 && _persistentCounter % 1000 == 0)
        //        _logger.LogDebug($"Analysed size of {_persistentCounter}/{filteredDirs.Count} directories");

        //    matches.Add(new DirData(dirAlreadyScanned.Path, GetTotalSizeInKb(filteredDirs, dirAlreadyScanned)));

        //    _persistentCounter++;
        //}

        matches = matches.OrderByDescending(dir => dir.Size).ToList();

        LogOutput(matches);

        _logger.LogDebug("Done. All tasks complete in {Time} sec/s", sw.ElapsedMilliseconds / 1000);

        return await Task.FromResult(0);
    }    

    private double GetTotalSizeInKb(List<DirData> dirs, DirData dirAlreadyScanned) => 
        dirs.Where(dir => dir.Path.Contains(dirAlreadyScanned.Path))
            .Select(dir => dir.Size / 1024)
            .Sum()
            .ToTwoDecimalPlaces();   
    
    private void LogOutput(List<DirData> matches)
    {
        int counter = 0;

        foreach (DirData dir in matches.Take(100))
        {
            double sizeMB = (dir.Size / 1024).ToTwoDecimalPlaces();
            double sizeGB = (sizeMB / 1024).ToTwoDecimalPlaces();
            _logger.LogDebug("#{Counter} Total Size for dir: {Dir} :: " + $"{dir.Size}KB" + ((dir.Size > 1024) ? $" {sizeMB}MB" : "") + ((sizeMB > 1024) ? $" {sizeGB}GB" : ""), ++counter, dir.Path, dir.Size);
        }
    }

    private void RecurseDirsFromPath(string path, List<DirData> dirData)
    {
        //if (_persistentCounter > 0 && _persistentCounter % 50000 == 0)
        //    _logger.LogDebug($"Scanned {_persistentCounter} directories");

        try
        {
            dirData.Add(new DirData(path, new DirectoryInfo(path).GetFiles().Select(file => file.Length).Sum()));
            //_persistentCounter++;
        }
        catch (Exception ex)
        {
            //_logger.LogWarning("Cant scan path: {Path}. Error: {ErrorMessage}", path, ex.Message);
            return;
        }

        Parallel.ForEach(Directory.GetDirectories(path), dir =>
        {
            RecurseDirsFromPath(dir, dirData);
        });

        //Directory.GetDirectories(path).ToList().ForEach(dir =>
        //{
        //    RecurseDirsFromPath(dir, dirData);
        //});
    }
}

public static class DoubleExtensions
{
    public static List<DirData> GetDirsDeep(this List<DirData> dirs, int searchLevelsDeep) =>
        dirs.Where(dir => new Regex($@"^[a-zA-Z]:\\[^\\/:*?""<>|\r\n]+(\\[^\\/:*?""<>|\r\n]+){{0,{searchLevelsDeep}}}\\?$")
        .Matches(dir.Path).Count > 0)
        .ToList();

    public static List<DirData> FilterWindowsDirectories(this List<DirData> dirs) =>
        dirs.Where(x => !x.Path.Contains("C:\\Windows")).ToList();
}

public static class DirectoryExtensions
{
    public static double ToTwoDecimalPlaces(this double num) => Math.Round(num, 2);
}

public class DirData
{
    public DirData(string path, double size)
    {
        Path = path;
        Size = size;
    }

    public string Path { get; set; }
    public double Size { get; set; }
}
