using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FindBiggestResources.BLL
{
    internal class InputParsingHelpers : IInputParsingHelpers
    {
        private readonly ILogger<IInputParsingHelpers> _logger;
        private const string DriveLetterRegex = @"^[A-Za-z]:\\$";

        public InputParsingHelpers(ILogger<IInputParsingHelpers> logger)
        {
            _logger = logger;
        }

        public (bool, string) ParseNextPath(List<DirData> largestDirsListed, string path)
        {
            string output = "\nEnter a number like \"1\" to drill down into a directory, or \"..\" to go back up (Or type \"exit\" to quit): ";

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
                    if (new Regex(DriveLetterRegex).IsMatch(path))
                    {
                        _logger.LogInformation($"Cannot move up a directory from {path} {output}");
                        continue;
                    }

                    _logger.LogInformation("Moving up a directory");
                    return (true, Path.GetDirectoryName(path) ?? throw new PathException(path));
                }

                bool success = int.TryParse(value, out int choice);
                if (!success || choice < 1 || choice > largestDirsListed.Count)
                {
                    _logger.LogInformation($"\n\"{value}\" is not a valid number {output}");
                    continue;
                }

                return (true, largestDirsListed[choice - 1].Path);
            }
        }
    }
}
