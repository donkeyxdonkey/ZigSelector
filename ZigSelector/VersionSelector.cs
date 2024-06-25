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
    private int _zigIndex;

    public VersionSelector()
    {
        _paths = GeneratePaths();
        _state = _paths.Length != 0 ? State.Active : State.Terminate;
        _config = Helper.GetConfiguration();
        _index = -1;
        _zigIndex = -1;

        ListBinaries();
        //UpdatePath(new DirectoryInfo("korv"));
    }

    private void ListBinaries()
    {
        bool pathFound = false;
        int index = 0;

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
                newPath.Append($"{ZigBinaries[_index].FullName}{SEPARATOR}");
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
