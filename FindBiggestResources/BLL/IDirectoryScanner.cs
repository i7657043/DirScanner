internal interface IDirectoryScanner
{
    void RecurseDirsFromPath(string path, List<DirData> dirData);
}
