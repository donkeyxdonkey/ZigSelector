using System.Diagnostics;
using ZigSelector;
using ZigSelector.Utility;

internal class Program
{
    const int MIN = 0;
    static int MAX = 0;

    private static void Main(string[] args)
    {
        try
        {
            using (DeferCursor.Defer())
            {
                VersionSelector zigSelector = new();
                KeyListener listener = new();
                MAX = zigSelector.Length - 1;

                while (zigSelector.State == State.Active)
                {
                    HandleKeyPress(listener.Listen(), ref zigSelector);
                }
            }

            ListVersion();
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.ReadLine();
        }
    }

    private static void HandleKeyPress(ConsoleKey consoleKey, ref VersionSelector zigSelector)
    {
        switch (consoleKey)
        {
            case ConsoleKey.Enter:
                EnterPressed(ref zigSelector);
                break;
            case ConsoleKey.X:
                ExitApplication(ref zigSelector);
                break;
            default:
                Navigate(consoleKey, ref zigSelector);
                break;
        }
    }

    private static void ExitApplication(ref VersionSelector zigSelector)
    {
        zigSelector.Terminate();
    }

    private static void Navigate(ConsoleKey consoleKey, ref VersionSelector zigSelector)
    {
        switch (consoleKey)
        {
            case ConsoleKey.UpArrow:
                if (zigSelector.Index == 0)
                    return;
                break;
            default:                                        /* DownArrow */
                if (zigSelector.Index == MAX)
                    return;
                break;
        }
        zigSelector.MoveCursor(consoleKey);
    }

    private static void EnterPressed(ref VersionSelector zigSelector)
    {
        zigSelector.UpdatePath();
    }

    static void ListVersion(string command = "zig", string arguments = "version")
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo)!)
            {
                if (process is null)
                    return;

                using (StreamReader reader = process.StandardOutput)
                {
                    Console.WriteLine($"Current Zig Version: {reader.ReadToEnd()}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running command '{command} {arguments}': {ex.Message}");
        }
    }
}