using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

internal class DirectoryScanner : IDirectoryScanner
{
    private readonly ILogger<IDirectoryScanner> _logger;

    public DirectoryScanner(ILogger<IDirectoryScanner> logger) =>
        _logger = logger;    

    public void RecurseDirsFromPath(string path, List<DirData> dirData)
    {
        if (path.Contains("C:\\Windows") || path.Contains("C:\\$Recycle.Bin"))
            return;

        try
        {
            dirData.Add(new DirData(path, GetFilesInDirSize(path)));
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Error: {Error}", ex.Message);
            return;
        }

        Parallel.ForEach(Directory.GetDirectories(path), dir =>
        {
            RecurseDirsFromPath(dir, dirData);
        });
    }

    private static long GetFilesInDirSize(string path) =>
        new DirectoryInfo(path).GetFiles().Select(file => file.Length).Sum();   

    public List<DirData> GetAllDirSizes(List<DirData> dirs)
    {
        ConcurrentBag<DirData> concurrentMatches = new ConcurrentBag<DirData>();

        Parallel.ForEach(dirs.DirsByDeep(), dir =>
        {
            lock (concurrentMatches)
            {
                if (concurrentMatches.Any(d => dir.Path.Contains(d.Path)))
                    return;
            }

            concurrentMatches.Add(new DirData(
                dir.Path,
                GetAllDirsForPath(dirs, dir.Path).GetTotalSizeInKb()));
        });

        return concurrentMatches.ToList();
    }

    private static List<DirData> GetAllDirsForPath(List<DirData> dirs, string path) =>
        dirs.Where(d => d.Path.Contains(path)).ToList();
}