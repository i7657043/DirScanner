namespace DirScanner.Services.Abstractions
{
    internal interface IInputParsingHelpers
    {
        string? GetNextPathFromUserInput(List<DirData> dirs, string path);
    }
}
