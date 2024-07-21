using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Runtime.CompilerServices;

internal class ResourceLister : IResourceLister
{
    private readonly ILogger<IResourceLister> _logger;

    public ResourceLister(ILogger<IResourceLister> logger)
    {
        _logger = logger;
    }

    public async Task<int> Run()
    {
        List<DirData> dirsToSearch = new List<DirData>();
        _logger.LogInformation("Scanning first pass...");
        GetAllDirsFirstPass(@"c:\fv", dirsToSearch);
        _logger.LogInformation("Done");

        List<DirData> dirData = new List<DirData>();

        foreach (DirData dirToSearch in dirsToSearch)
        {
            //_logger.LogInformation($"Scanning second pass dir: {dirToSearch.Path} ...");
            GetAllDirsSecondPass(dirToSearch.Path, dirData, dirsToSearch);
            //_logger.LogInformation("Done");

            double size = dirData.Select(dir => dir.Size / 1024).Sum().ToTwoDecimalPlaces();
            double sizeMB = (size / 1024).ToTwoDecimalPlaces();
            double sizeGB = (sizeMB / 1024).ToTwoDecimalPlaces();

            //_logger.LogInformation("Total Size for dir #{Counter}: {Dir} :: " + $"{size}KB" + ((size > 1024) ? $" {sizeMB}MB" : "") + ((sizeMB > 1024) ? $" {sizeGB}GB" : ""), ++outerCounter, dirToSearch.Path, size);
        }

        dirData = dirData.OrderBy(dir => dir.Size).ToList();

        return await Task.FromResult(0);
    }

    private void GetAllDirsFirstPass(string path, List<DirData> dirData)
    {
        FileInfo[] files = new DirectoryInfo(path).GetFiles();
        dirData.Add(new DirData(path, files.Select(file => file.Length).Sum()));

        Directory.GetDirectories(path).ToList().ForEach(dir =>
        {
            GetAllDirsFirstPass(dir, dirData);
        });
    }

    private void GetAllDirsSecondPass(string path, List<DirData> dirData, List<DirData> scannedDirs)
    {
        DirData alreadyScannedDir = scannedDirs.First(dir => dir.Path == path);
        dirData.Add(alreadyScannedDir);
        
        Directory.GetDirectories(path).ToList().ForEach(dir =>
        {
            GetAllDirsSecondPass(dir, dirData, scannedDirs);
        });
    }
}

public static class DoubleExtensions
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
