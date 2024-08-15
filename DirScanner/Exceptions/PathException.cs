public class PathException : Exception 
{
    public PathException(string path)
    {
        Path = path;
    }

    public string Path { get; } = string.Empty;
}