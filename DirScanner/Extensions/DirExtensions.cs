public static class DirExtensions
{
    public static List<DirData> FilterTopMatchesBySize(this List<DirData> dirs)
    {
        //change to be a select
        List<DirData> topSizeDirs = new List<DirData>();
        foreach (DirData dir in DirsByDeep(dirs))
        {
            if (!topSizeDirs.Any(d => dir.Path.Contains(d.Path)))
                topSizeDirs.Add(dir);
        }

        return topSizeDirs.OrderByDescending(dir => dir.Size).ToList();
    }

    public static IOrderedEnumerable<DirData> DirsByDeep(this List<DirData> dirs) =>
        dirs.OrderBy(x => x.Path.Count(p => p == '\\'));    

    public static double GetTotalSizeInKb(this List<DirData> dirs) =>
        dirs.Select(dir => dir.Size / 1024)
            .Sum()
            .ToTwoDecimalPlaces();
}
