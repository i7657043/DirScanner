using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirScanner.Extensions
{
    public static class CommandLineOptionsExtensions
    {
        public static CommandLineOptions ParseCommandLineOptions(this Options options)
        {
            CommandLineOptions commandLineOptions = new CommandLineOptions();

            if (!string.IsNullOrEmpty(options.Path))
            {
                commandLineOptions.Path = CapitaliseDriveLetterOfPath(options.Path);
            }
            else
            {
                string? path = GetPathFromInput();
                if (path != null)
                    commandLineOptions.Path = path;
                else
                    Log.Logger.Information($"Using default path: {commandLineOptions.Path} as the root of the search");
            }

            if (!Directory.Exists(commandLineOptions.Path))
                throw new PathException(commandLineOptions.Path);

            commandLineOptions.OutputSize = options.Size > 0 ? options.Size : commandLineOptions.OutputSize;

            return commandLineOptions;
        }

        private static string CapitaliseDriveLetterOfPath(string path)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(char.ToUpper(path[0]));
            sb.Append(path.Substring(1));
            return sb.ToString();
        }

        private static string? GetPathFromInput()
        {
            Console.Write("Please enter a path to use as the root of the search (or hit enter to use the default): ");
            string path = Console.ReadLine() ?? string.Empty;
            if (!string.IsNullOrEmpty(path))
                return path;

            return null;
        }
    }
}
