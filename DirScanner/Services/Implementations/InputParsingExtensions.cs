using DirScanner.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DirScanner.Services.Implementations
{
    internal class InputParsingHelpers : IInputParsingHelpers
    {
        private const string exitCommand = "exit";
        private const string upDirCommand = "..";
        private readonly ILogger<IInputParsingHelpers> _logger;

        public InputParsingHelpers(ILogger<IInputParsingHelpers> logger) =>
            _logger = logger;        

        public string? GetNextPathFromUserInput(List<DirData> largestDirsListed, string path)
        {
            string output = $"Enter a number like \"1\" to move into a directory, or \"{upDirCommand}\" to go up. Or type \"{exitCommand}\" to quit: ";

            _logger.LogInformation(output);

            while (true)
            {
                string? value = Console.ReadLine();
                if (value?.ToLower() == exitCommand)
                {
                    _logger.LogInformation("Exiting");

                    return null;
                }
                else if (value?.ToLower() == upDirCommand)
                {
                    //Drive letter regex i.e. matches absolute paths with drive letter i.e. "c:\" but not relative paths i.e. dir\dir
                    if (new Regex(@"^[A-Za-z]:\\$").IsMatch(path))
                    {
                        _logger.LogInformation($"Cannot move up from the root of this directory structure: {path}\n{output}");
                        continue;
                    }

                    _logger.LogInformation("Moving up a directory");
                    return Path.GetDirectoryName(path) ?? throw new PathException(path);
                }

                if (!int.TryParse(value, out int choice) || choice < 1 || choice > largestDirsListed.Count)
                {
                    _logger.LogInformation($"\"{value}\" is not a valid selection, it must be a number and in the list\n{output}");
                    continue;
                }

                return largestDirsListed[choice - 1].Path;
            }
        }
    }
}
