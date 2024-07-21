using Microsoft.Extensions.Logging;

internal class ResourceLister : IResourceLister
{
    private readonly ILogger<IResourceLister> _logger;

    public ResourceLister(ILogger<IResourceLister> logger)
    {
        _logger = logger;
    }

    public async Task<int> Run()
    {
        await Task.Delay(1000);

        TraverseAll(@"c:\fv\n");

        return await Task.FromResult(0);
    }

    private List<DirData> TraverseAll(string path)
    {
        List<DirData> allDirs = new List<DirData>();

        Directory.GetDirectories(path).ToList().ForEach(dir =>
        {        
            _logger.LogInformation("--- Traversing entire dir: {Dir} ---", dir);

            List<DirData> data = GetEntireDirData(dir);

            double dirSize = data.Select(x => x.Size).Sum();
        
            _logger.LogInformation("/// Traversal of entire dir: {Dir} complete. Size: {DirSize} KB ///\n", dir, dirSize);
        });

        return allDirs;
    }

    private List<DirData> GetEntireDirData(string path)
    {
        List<DirData> allDirsForThisDir = new List<DirData>();

        allDirsForThisDir = TraverseDirectories(path, allDirsForThisDir, 0, 0);

        return allDirsForThisDir;
    }

    private List<DirData> TraverseDirectories(string path, List<DirData> allDirs, int currentLevelsDeep, int totalNumberOfFiles)
    {
        currentLevelsDeep++;

        FileInfo[] files = new DirectoryInfo(path).GetFiles();

        int numOfFilesInDir = files.Length;

        totalNumberOfFiles += numOfFilesInDir;

        _logger.LogInformation("{FileCount} files in this Dir", numOfFilesInDir);

        allDirs.Add(new DirData(path, GetAllFilesSize(files)));

        Directory.GetDirectories(path).ToList().ForEach(dir => 
        {
            _logger.LogInformation("Traversing dir: {Dir}", dir);

            TraverseDirectories(dir, allDirs, currentLevelsDeep, totalNumberOfFiles);            
        });

        return allDirs;
    }

    public double GetAllFilesSize(FileInfo[] files)
    {
        double totalKBytes = 0;

        files.ToList().ForEach(file => totalKBytes += file.Length / 1024);

        return totalKBytes;
    }
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

