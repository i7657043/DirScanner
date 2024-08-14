public interface IProgressBar
{
    void Start();
    Task Stop();
}

public class ProgressBar : IProgressBar
{
    private volatile bool processing = false;
    private Task? task;
    private int counter = 0;

    public void Start()
    {
        if (!processing)
        {
            processing = true;
            task = Task.Run(() => ShowProgress());
        }
    }

    public async Task Stop()
    {
        processing = false;
        if (task != null)
            await task;
    }

    private async Task ShowProgress()
    {
        while (processing)
        {
            switch (counter)
            {
                case 0:
                    Console.Write("/");
                    break;

                case 1:
                    Console.Write("-");
                    break;

                case 2:
                    Console.Write("\\");
                    break;

                case 3:
                    Console.Write("|");
                    break;

                case 4:
                    Console.Write("/");
                    break;

                case 5:
                    Console.Write("-");
                    break;

                case 6:
                    Console.Write("\\");
                    break;

                case 7:
                    Console.Write("|");
                    counter = 0;
                    break;
            }

            counter++;
            await Task.Delay(100);
            Console.Write("\b \b");
        }
    }
}
