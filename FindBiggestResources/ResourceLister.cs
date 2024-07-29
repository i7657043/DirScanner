using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

internal class ResourceLister : IResourceLister
{
    private readonly ILogger<IResourceLister> _logger;

    public ResourceLister(ILogger<IResourceLister> logger)
    {
        _logger = logger;
    }

    public async Task<int> Run()
    {
        List<DirData> dirs = new List<DirData>();

        //inputs
        string startPath = @"C:\";
        int searchLevelsDeep = 6;
        int numOfFilesToShowInOutput = 35;

        _logger.LogDebug("Recursing all directories...");

        RecurseDirsFromPath(startPath, dirs);

        dirs.RemoveAt(0);

        List<DirData> matches = new List<DirData>();

        _logger.LogDebug("Finding largest directories...");

        List<DirData> filteredDirs = dirs.GetDirsDeep(searchLevelsDeep).FilterWindowsDirectories();

        _logger.LogDebug("Done. Found {NoOfDirs} directories. {FilteredDirsCount} will be analysed", dirs.Count, filteredDirs.Count);

        Parallel.ForEach(filteredDirs, dirAlreadyScanned =>
            matches.Add(new DirData(dirAlreadyScanned.Path, GetTotalSizeInKb(filteredDirs.Where(dir => dir.Path.Contains(dirAlreadyScanned.Path)).ToList()))));

        LogOutput(matches.OrderByDescending(dir => dir.Size).ToList(), numOfFilesToShowInOutput);

        _logger.LogDebug("All operations complete");

        return await Task.FromResult(0);
    }    

    private double GetTotalSizeInKb(List<DirData> dirs) => 
        dirs.Select(dir => dir.Size / 1024)
            .Sum()
            .ToTwoDecimalPlaces();
    
    private void LogOutput(List<DirData> matches, int numOfFilesToShowInOutput)
    {
        int counter = 0;
        double totalSize = 0;

        foreach (DirData dir in matches.Take(numOfFilesToShowInOutput))
        {
            totalSize += dir.Size;
            double sizeMB = (dir.Size / 1024).ToTwoDecimalPlaces();
            double sizeGB = (sizeMB / 1024).ToTwoDecimalPlaces();
            //make this formatted logging
            _logger.LogInformation("#{Counter} Total Size for dir: {Dir} :: " + $"{dir.Size}KB" + ((dir.Size > 1024) ? $" {sizeMB}MB" : "") + ((sizeMB > 1024) ? $" {sizeGB}GB" : ""), ++counter, dir.Path, dir.Size);
        }

        _logger.LogDebug("Total Size for all files listed: {Size}GB", ((totalSize / 1024) / 1024).ToTwoDecimalPlaces());
    }

    private void RecurseDirsFromPath(string path, List<DirData> dirData)
    {
        try
        {
            dirData.Add(new DirData(path, new DirectoryInfo(path).GetFiles().Select(file => file.Length).Sum()));
        }
        catch (Exception ex)
        {
            return;
        }

        Parallel.ForEach(Directory.GetDirectories(path), dir =>
        {
            RecurseDirsFromPath(dir, dirData);
        });
    }
}

public static class DoubleExtensions
{
    //add this path to the recurse method above, and see if we can continue if we are searching a path too deep
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
