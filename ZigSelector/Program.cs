using System.Diagnostics;
using System.IO;
using ZigSelector;
using ZigSelector.Utility;

internal class Program
{
    const int MIN = 2;
    static int MAX = 0;

    private static void Main(string[] args)
    {
        try
        {
            ParseArgs(args);

            string newVersion = string.Empty;
            bool exitPressed = false;

            using (DeferCursor.Defer())
            {
                VersionSelector zigSelector = new(offset: MIN);
                KeyListener listener = new();
                MAX = zigSelector.Length + 1;

                while (zigSelector.State == State.Active)
                {
                    HandleKeyPress(listener.Listen(), ref zigSelector, out exitPressed);
                }

                newVersion = zigSelector.SelectedVersion;
            }

            if (exitPressed)
                return;

            RunCommand("zig", "version", printStdOut: true, message: "Current Zig Version: ");

            Console.WriteLine(newVersion);
            Console.Write("For changes to take effect run: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("update_environment.ps1");
            Console.ForegroundColor = ConsoleColor.White;
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

    private static void ParseArgs(string[] args)
    {
        if (args.Length == 0)
            return;

        const string PATH = "PATH";

        Console.WriteLine($"BEGIN Reading args{Environment.NewLine}");

        foreach (string arg in args.Select(arg => arg.ToLower()).ToArray())
        {
            switch (arg)
            {
                case "--help":
                    Console.WriteLine("COMMANDS\n\n");
                    Console.WriteLine("--help: command information.");
                    Console.WriteLine("--add-to-path: generates path for ZigSelector at it's current location.");
                    break;
                case "--add-to-path":
                    string? paths = Environment.GetEnvironmentVariable(PATH, EnvironmentVariableTarget.User);
                    DirectoryInfo cwd = new(AppContext.BaseDirectory);
                    if (paths is null || paths.Contains(cwd.FullName)) // #1 enbart för att slippa nullchecks
                    {
                        Console.WriteLine("Already added to path.");
                        break;
                    }

                    Environment.SetEnvironmentVariable(PATH, string.Concat(paths, ";", cwd.FullName), EnvironmentVariableTarget.User);
                    Console.WriteLine($"Path added: {cwd.FullName}");
                    break;
                default:
                    Console.WriteLine($"Unknown argument: {arg}");
                    return;
            }
        }

        Console.WriteLine("END Reading args");
        Console.ReadLine();
        Console.Clear();
        Console.SetCursorPosition(0, 0);
    }

    private static void HandleKeyPress(ConsoleKey consoleKey, ref VersionSelector zigSelector, out bool exitPressed)
    {
        exitPressed = false;
        switch (consoleKey)
        {
            case ConsoleKey.Enter:
                EnterPressed(ref zigSelector);
                break;
            case ConsoleKey.X:
                ExitApplication(ref zigSelector, out exitPressed);
                break;
            default:
                Navigate(consoleKey, ref zigSelector);
                break;
        }
    }

    private static void ExitApplication(ref VersionSelector zigSelector, out bool exitPressed)
    {
        exitPressed = true;
        zigSelector.Terminate();
    }

    private static void Navigate(ConsoleKey consoleKey, ref VersionSelector zigSelector)
    {
        switch (consoleKey)
        {
            case ConsoleKey.UpArrow:
                if (zigSelector.Index == MIN)
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

    static void RunCommand(string command, string arguments, bool printStdOut = false, string message = "")
    {
        const string VERB = "runas";

        try
        {
            ProcessStartInfo startInfo = new()
            {
                Verb = VERB,
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

                if (!printStdOut)
                {
                    process.WaitForExit();
                    return;
                }

                using (StreamReader reader = process.StandardOutput)
                {
                    Console.WriteLine($"{message}{reader.ReadToEnd()[..^1]}"); // ta bort sista charren på readern som är \n
                }

                process.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running command '{command} {arguments}': {ex.Message}");
        }
    }
}