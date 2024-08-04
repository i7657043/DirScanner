public static class DirExtensions
{
    public static List<DirData> FilterTopMatchesBySize(this List<DirData> dirs)
    {
        List<DirData> topSizeDirs = new List<DirData>();
        foreach (DirData dir in dirs.OrderBy(x => x.Path.Count(p => p == '\\')))
        {
            if (!topSizeDirs.Any(d => dir.Path.Contains(d.Path)))
                topSizeDirs.Add(dir);
        }

        return topSizeDirs.OrderByDescending(dir => dir.Size).ToList();
    }

}
