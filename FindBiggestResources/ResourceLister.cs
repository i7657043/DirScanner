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
        TraverseAll(@"c:\fv\n");

        return await Task.FromResult(0);
    }

    private void TraverseAll(string path)
    {
        Directory.GetDirectories(path).ToList().ForEach(dir =>
        {        
            _logger.LogDebug("--- Traversing entire dir: {Dir} ---", dir);

            List<DirData> data = GetEntireDirData(dir);

            double dirSize = data.Select(x => x.Size).Sum();
        
            _logger.LogDebug("/// Traversal of entire dir: {Dir} complete. Size: {DirSize} KB ///", dir, dirSize);
        });
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

        _logger.LogDebug("{FileCount} file/s found", numOfFilesInDir);

        allDirs.Add(new DirData(path, GetAllFilesSize(files)));

        Directory.GetDirectories(path).ToList().ForEach(dir => 
        {
            _logger.LogDebug("Traversing dir: {Dir}", dir);

            TraverseDirectories(dir, allDirs, currentLevelsDeep, totalNumberOfFiles); //should this be get entire dir data?
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

