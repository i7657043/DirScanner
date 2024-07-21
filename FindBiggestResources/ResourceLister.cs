using Microsoft.Extensions.Logging;
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
        int counter = 0;
        List<string> dirsToSearch = new List<string>();
        GetAllDirs(@"c:\fv\n", dirsToSearch);
        int outerCounter = 0;

        foreach (string dirToSearch in dirsToSearch)
        {
            counter = 0; 
            _logger.LogInformation("---Dir to find total for dir #{Counter}: {Dir}---", ++outerCounter, dirToSearch);

            List<DirData> dirData = new List<DirData>();

            GetAllDirsWithTotals(dirToSearch, dirData);
            
            dirData.ForEach(dir => _logger.LogInformation("Cumulating total for dir #{Counter}: {Dir} :: Total Size: {Size} KB", ++counter, dir.Path, dir.Size.ToKBTwoDecimalPlaces()));
            _logger.LogInformation("---Total Size for dir #{Counter}: {Dir} :: Total Size: {Size} KB---", outerCounter, dirToSearch, dirData.Select(dir => dir.Size).Sum().ToKBTwoDecimalPlaces());
        }

        return await Task.FromResult(0);
    }

    private void GetAllDirsWithTotals(string path, List<DirData> dirData)
    {
        FileInfo[] files = new DirectoryInfo(path).GetFiles();
        dirData.Add(new DirData(path, files.Select(file => file.Length).Sum()));

        Directory.GetDirectories(path).ToList().ForEach(dir =>
        {
            GetAllDirsWithTotals(dir, dirData);
        });
    }

    private void GetAllDirs(string path, List<string> allDirs)
    {
        allDirs.Add(path);

        Directory.GetDirectories(path).ToList().ForEach(dir =>
        {
            GetAllDirs(dir, allDirs);
        });
    }
}

public static class DoubleExtensions
{
    public static double ToKBTwoDecimalPlaces(this double num) => Math.Round(num / 1024, 2);
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
