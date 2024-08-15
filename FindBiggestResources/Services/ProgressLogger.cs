using Microsoft.Extensions.Logging;
using System.IO;

namespace FindBiggestResources.Services
{
    internal class ProgressLogger : IProgressLogger
    {
        private readonly ILogger<IResourceLister> _logger;
        private readonly IProgressBar _progresssBar;

        public ProgressLogger(ILogger<IResourceLister> logger, IProgressBar progresssBar)
        {
            _logger = logger;
            _progresssBar = progresssBar;
        }

        public void LogRecursing(string path)
        {
            _logger.LogInformation($"Recursing all directories from {path}");

            _progresssBar.Start();
        }

        public async Task LogRecursedAsync(int dirCount)
        {
            await _progresssBar.Stop();

            _logger.LogInformation("Done. Found {NoOfDirs} directories to be analysed. Scanning for largest directories", dirCount);
        }

        public void LogAnalysing()
        {
            _progresssBar.Start();
        }

        public async Task LogAnalysed(List<DirData> dirs, int outputSize, long timeElapsedSeconds)
        {
            await _progresssBar.Stop();

            LogDirBySize(dirs, outputSize);

            _logger.LogInformation("All operations complete in {Time} sec/s", timeElapsedSeconds);
        }

        private void LogDirBySize(List<DirData> matches, int numOfFilesToShowInOutput)
        {
            int counter = 0;
            _logger.LogInformation($"{"#",-3} {"Path",-60} {"Size"}");

            foreach (DirData dir in matches.Take(numOfFilesToShowInOutput))
            {
                double sizeMB = dir.Size / 1024;
                double sizeGB = sizeMB / 1024;
                string dirSize = sizeMB > 1024 ? $"{sizeGB.ToTwoDecimalPlaces()}GB" : dir.Size > 1024 ? $"{sizeMB.ToTwoDecimalPlaces()}MB" : $"{dir.Size.ToTwoDecimalPlaces()}KB";

                _logger.LogInformation($"{++counter,-3} {dir.Path,-60} {dirSize}");
            }
        }
    }
}
