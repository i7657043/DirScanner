using System.Collections.Concurrent;

internal class DirectoryScanner : IDirectoryScanner
{
    public void RecurseDirsFromPath(string path, List<DirData> dirData)
    {
        if (path.Contains("C:\\Windows") || path.Contains("C:\\$Recycle.Bin"))
            return;

        try
        {
            dirData.Add(new DirData(path, new DirectoryInfo(path).GetFiles().Select(file => file.Length).Sum()));
        }
        catch (Exception)
        {
            return;
        }

        Parallel.ForEach(Directory.GetDirectories(path), dir =>
        {
            RecurseDirsFromPath(dir, dirData);
        });
    }

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