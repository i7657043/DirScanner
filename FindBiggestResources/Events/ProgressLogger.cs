using Microsoft.Extensions.Logging;
using System.IO;

namespace FindBiggestResources.Events
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

        public void OnRecursing(object? sender, ProgressEventArgs e)
        {
            _logger.LogInformation($"Recursing all directories from {e.Path}");

            _progresssBar.Start();
        }

        public async Task OnRecursedAsync(object? sender, RecurseFinishedProgressEventArgs e)
        {
            await _progresssBar.Stop();

            _logger.LogInformation("Done. Found {NoOfDirs} directories to be analysed. Scanning for largest directories", e.DirCount);
        }

        public void OnAnalysing(object? sender, EventArgs e)
        {
            _progresssBar.Start();
        }

        public async Task OnAnalysed(object? sender, AnalysingFinishedProgressEventArgs e)
        {
            await _progresssBar.Stop();

            LogDirBySize(e.Dirs, e.OutputSize);

            _logger.LogInformation("All operations complete in {Time} sec/s", e.TimeElapsedSeconds);
        }

        private void LogDirBySize(List<DirData> matches, int numOfFilesToShowInOutput)
        {
            int counter = 0;
            _logger.LogInformation($"{"#",-3} {"Path",-60} {"Size"}");

            foreach (DirData dir in matches.Take(numOfFilesToShowInOutput))
            {
                double sizeMB = dir.Size / 1024;
                double sizeGB = sizeMB / 1024;
                string dirSize = (sizeMB > 1024) ? $"{sizeGB.ToTwoDecimalPlaces()}GB" : ((dir.Size > 1024) ? $"{sizeMB.ToTwoDecimalPlaces()}MB" : $"{dir.Size.ToTwoDecimalPlaces()}KB");

                _logger.LogInformation($"{++counter,-3} {dir.Path,-60} {dirSize}");
            }
        }
    }

    internal interface IProgressLogger
    {
        void OnRecursing(object? sender, ProgressEventArgs e);
        Task OnRecursedAsync(object? sender, RecurseFinishedProgressEventArgs e);
        void OnAnalysing(object? sender, EventArgs e);
        Task OnAnalysed(object? sender, AnalysingFinishedProgressEventArgs e);
    }

    internal class ProgressEventArgs : EventArgs
    {
        public string Path { get; set; }

        public ProgressEventArgs(string path)
        {
            Path = path;
        }
    }

    internal class RecurseFinishedProgressEventArgs : EventArgs
    {
        public int DirCount { get; set; }

        public RecurseFinishedProgressEventArgs(int dirCount)
        {
            DirCount = dirCount;
        }
    }

    internal class AnalysingFinishedProgressEventArgs : EventArgs
    {
        public List<DirData> Dirs { get; set; }
        public int OutputSize { get; set; }
        public long TimeElapsedSeconds { get; set; }

        public AnalysingFinishedProgressEventArgs(List<DirData> dirs, int outputSize, long timeElapsedSeconds)
        {
            Dirs = dirs;
            OutputSize = outputSize;
            TimeElapsedSeconds = timeElapsedSeconds;
        }
    }
}
