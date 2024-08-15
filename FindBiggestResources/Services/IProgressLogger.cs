namespace FindBiggestResources.Services
{
    internal interface IProgressLogger
    {
        void LogRecursing(string path);
        Task LogRecursedAsync(int dirCount);
        void LogAnalysing();
        Task LogAnalysed(List<DirData> dirs, int outputSize, long timeElapsedSeconds);
    }
}
