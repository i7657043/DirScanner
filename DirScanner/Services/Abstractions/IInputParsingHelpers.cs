namespace DirScanner.Services.Abstractions
{
    internal interface IInputParsingHelpers
    {
        (bool, string) GetUserInputForWhichPathToDrillInto(List<DirData> largestDirsListed, string path);
    }
}
