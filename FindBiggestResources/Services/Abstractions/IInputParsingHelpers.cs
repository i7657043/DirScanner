namespace FindBiggestResources.Services.Abstractions
{
    internal interface IInputParsingHelpers
    {
        (bool, string) GetUserInputForWhichPathToDrillInto(List<DirData> largestDirsListed, string path);
    }
}
