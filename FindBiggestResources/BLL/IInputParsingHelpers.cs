namespace FindBiggestResources.BLL
{
    internal interface IInputParsingHelpers
    {
        (bool, string) ParseNextPath(List<DirData> largestDirsListed, string path);
    }
}
