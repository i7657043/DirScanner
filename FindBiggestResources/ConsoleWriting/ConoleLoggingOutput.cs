internal static class ConoleLoggingOutput
{
    public static bool IsProcessing;

    public static Task ShowWaitOutputAsync()
    {
        IsProcessing = true;
        Task.Run(async () =>
        {
            while (IsProcessing)
            {
                Console.Write(".");
                await Task.Delay(500);
            }
        });

        return Task.CompletedTask;
    }    
}
