internal interface IDirectoryScanner
{
    void RecurseDirsFromPath(string path, List<DirData> dirData);
    List<DirData> GetAllDirSizes(List<DirData> dirs);
}
