using Microsoft.Extensions.Logging;
using System.Diagnostics;

internal class ResourceLister : IResourceLister
{
    private readonly ILogger<IResourceLister> _logger;
    private readonly CommandLineOptions _commandLineOptions;

    public ResourceLister(ILogger<IResourceLister> logger, CommandLineOptions commandLineOptions)
    {
        _logger = logger;
        _commandLineOptions = commandLineOptions;
    }

    public async Task<int> RunAsync()
    {
        Stopwatch sw = Stopwatch.StartNew();
        List<DirData> dirs = new List<DirData>();

        _logger.LogInformation($"Recursing all directories from {_commandLineOptions.Path}");

        bool isProcessing = true;
        Task waitOutputTask = ShowWaitOutputAsync(() => isProcessing);

        RecurseDirsFromPath(_commandLineOptions.Path, dirs);
        dirs.RemoveAt(0);

        isProcessing = false;
        await waitOutputTask;
        Console.WriteLine();        

        _logger.LogInformation("Done. Found {NoOfDirs} directories to be analysed. Scanning for largest directories", dirs.Count);

        List<DirData> matches = new List<DirData>();

        isProcessing = true;
        waitOutputTask = ShowWaitOutputAsync(() => isProcessing);

        Parallel.ForEach(dirs, dir => matches.Add(new DirData(dir.Path, GetTotalSizeInKb(dirs.Where(d => d.Path.Contains(dir.Path)).ToList()))));

        isProcessing = false;
        await waitOutputTask;
        Console.WriteLine();

        LogOutput(matches.FilterTopMatchesBySize(), _commandLineOptions.OutputSize);

        _logger.LogInformation("All operations complete in {Time} sec/s", sw.ElapsedMilliseconds / 1000);

        return await Task.FromResult(0);
    }

    private Task ShowWaitOutputAsync(Func<bool> isProcessingFunc)
    {
        return Task.Run(async () =>
        {
            while (isProcessingFunc())
            {
                Console.Write(".");
                await Task.Delay(500);
            }
        });
    }

    private double GetTotalSizeInKb(List<DirData> dirs) => 
        dirs.Select(dir => dir.Size / 1024)
            .Sum()
            .ToTwoDecimalPlaces();
    
    private void LogOutput(List<DirData> matches, int numOfFilesToShowInOutput)
    {
        int counter = 0;
        double totalSize = 0;
        string output = $"{"#",-3} {"Path",-60} {"Size"}";
        _logger.LogInformation(output);

        foreach (DirData dir in matches.Take(numOfFilesToShowInOutput))
        {
            totalSize += dir.Size;
            double sizeMB = (dir.Size / 1024).ToTwoDecimalPlaces();
            double sizeGB = (sizeMB / 1024).ToTwoDecimalPlaces();
            string dirSize = (sizeMB > 1024) ? $"{sizeGB}GB" : ((dir.Size > 1024) ? $"{sizeMB}MB" : $"{dir.Size}KB");            

            output = $"{++counter,-3} {dir.Path,-60} {dirSize}";
            _logger.LogInformation(output);
        }

        _logger.LogInformation("Total Size for all files listed: {Size}GB", ((totalSize / 1024) / 1024).ToTwoDecimalPlaces());
    }

    private void RecurseDirsFromPath(string path, List<DirData> dirData)
    {
        if (path.Contains("C:\\Windows"))
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
