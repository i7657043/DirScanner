public class DirData
{
    public DirData(string path, double size)
    {
        Path = path;
        Size = size;
    }

    public string Path { get; set; }
    public double Size { get; set; }
}
