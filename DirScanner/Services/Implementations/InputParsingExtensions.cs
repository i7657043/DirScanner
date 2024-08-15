using DirScanner.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DirScanner.Services.Implementations
{
    internal class InputParsingHelpers : IInputParsingHelpers
    {
        private readonly ILogger<IInputParsingHelpers> _logger;

        public InputParsingHelpers(ILogger<IInputParsingHelpers> logger) =>
            _logger = logger;        

        public (bool, string) GetUserInputForWhichPathToDrillInto(List<DirData> largestDirsListed, string path)
        {
            string output = "Enter a number like \"1\" to drill down into a directory, or \"..\" to go back up (Or type \"exit\" to quit): ";

            _logger.LogInformation(output);

            while (true)
            {
                string? value = Console.ReadLine();
                if (value?.ToLower() == "exit")
                {
                    _logger.LogInformation("Exiting");
                    return (false, path);
                }
                else if (value?.ToLower() == "..")
                {
                    //Drive letter regex i.e. matches "C:\" but not a\b
                    if (new Regex(@"^[A-Za-z]:\\$").IsMatch(path))
                    {
                        _logger.LogInformation($"Cannot move up a directory from {path}\n{output}");
                        continue;
                    }

                    _logger.LogInformation("Moving up a directory");
                    return (true, Path.GetDirectoryName(path) ?? throw new PathException(path));
                }

                bool success = int.TryParse(value, out int choice);
                if (!success || choice < 1 || choice > largestDirsListed.Count)
                {
                    _logger.LogInformation($"\"{value}\" is not a valid number\n{output}");
                    continue;
                }

                return (true, largestDirsListed[choice - 1].Path);
            }
        }
    }
}
