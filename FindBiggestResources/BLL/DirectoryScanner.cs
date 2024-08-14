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
}