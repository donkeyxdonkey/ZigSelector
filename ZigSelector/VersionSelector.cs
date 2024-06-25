using System.Text;
using ZigSelector.ConfigModel;
using ZigSelector.Utility;

namespace ZigSelector;

public class VersionSelector
{
    const char SEPARATOR = ';';
    const string PATH = "PATH";
    const string CLEAR_CURSOR = "   ";
    const string CURSOR = ">>";
    const string ERASE_CURSOR = "  ";

    public State State { get => _state; }

    public int Index { get => _index; set => _index = value; }

    public int Length => ZigBinaries.Length;

    private DirectoryInfo[] ZigBinaries
    {
        get
        {
            if (!Directory.Exists(_config.BinaryFolder))
                throw new DirectoryNotFoundException("");

            return new DirectoryInfo(_config.BinaryFolder).GetDirectories("*");
        }
    }

    private State _state;
    private readonly DirectoryInfo[] _paths;
    private readonly Configuration _config;

    private DirectoryInfo? _currentBinary;
    private int _index;
    private int _offset;
    private int _zigIndex;

    public VersionSelector(int offset)
    {
        _paths = GeneratePaths();
        _state = _paths.Length != 0 ? State.Active : State.Terminate;
        _config = Helper.GetConfiguration();
        _index = -1;
        _zigIndex = -1;

        ListBinaries();
        _index += offset;
        _offset = offset;
    }

    private void ListBinaries()
    {
        bool pathFound = false;
        int index = 0;

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write(" ++++++++++++++++++++ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("ZIG PATH SELECTOR");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(" ++++++++++++++++++++\n");
        Console.ForegroundColor = ConsoleColor.White;

        foreach (DirectoryInfo zigBin in ZigBinaries)
        {
            if (!pathFound)
            {
                LocateZigPath(zigBin, out pathFound);
                _index = index;
            }

            if (_currentBinary is not null && zigBin.FullName == _currentBinary.FullName)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($" {CURSOR}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
                Console.Write($" {ERASE_CURSOR}");

            Console.WriteLine($" {zigBin.FullName}");
            index++;
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write($"{Environment.NewLine} Keys: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Enter, UpArrow, DownArrow ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("Exit: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("X");
    }

    private void LocateZigPath(DirectoryInfo zigBin, out bool pathFound)
    {
        pathFound = false;

        for (int i = 0; i < _paths.Length; i++)
        {
            if (_paths[i].FullName != zigBin.FullName)
                continue;

            _zigIndex = i;
            _currentBinary = _paths[i];
            pathFound = true;
            break;
        }
    }

    private DirectoryInfo[] GeneratePaths()
    {
        string? paths = Environment.GetEnvironmentVariable(PATH, EnvironmentVariableTarget.User);
        if (paths is null)
            return [];

        return paths.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries).Select(x => new DirectoryInfo(x)).ToArray();
    }

    public void UpdatePath()
    {
        StringBuilder newPath = new();

        for (int i = 0; i < _paths.Length; i++)
        {
            if (i == _zigIndex)
            {
                newPath.Append($"{ZigBinaries[_index - _offset].FullName}{SEPARATOR}");
                continue;
            }

            newPath.Append($"{_paths[i].FullName};");
        }

        newPath.Length--; // ta bort sista ;

        try
        {
            Environment.SetEnvironmentVariable(PATH, newPath.ToString(), EnvironmentVariableTarget.User);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.ReadLine();
        }

        _state = State.Terminate;
    }

    internal void MoveCursor(ConsoleKey consoleKey)
    {
        Console.SetCursorPosition(1, _index);
        Console.Write(CLEAR_CURSOR);

        switch (consoleKey)
        {
            case ConsoleKey.DownArrow:
                _index++;
                break;
            case ConsoleKey.UpArrow:
                _index--;
                break;
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.SetCursorPosition(1, _index);
        Console.Write(CURSOR);
        Console.ForegroundColor = ConsoleColor.White; // kanske inte behövs...
    }

    internal void Terminate()
    {
        _state = State.Terminate;
    }
}
