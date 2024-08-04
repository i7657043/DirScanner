using CommandLine;

public class Options
{
    [Option('s', "outsize", Required = false, HelpText = "Number of records to show in output. Default is 35")]
    public int Size { get; set; }

    [Option('p', "path", Required = false, HelpText = @"Path to root of search. Default is C:\")]
    public string Path { get; set; } = string.Empty;

    [Option('v', "verbose", Required = false, HelpText = "Verbose logging. Default is off")]
    public bool Verbose { get; set; }
}
