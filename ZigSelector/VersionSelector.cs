using System.Text;
using ZigSelector.ConfigModel;
using ZigSelector.Utility;

namespace ZigSelector;

public class VersionSelector
{
    const string PATH = "PATH";

    public State State { get => _state; }

    public int Index { get => _index; set => _index = value; }

    private readonly State _state;
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
        if (!Directory.Exists(_config.BinaryFolder))
            throw new DirectoryNotFoundException("");

        bool pathFound = false;

        int index = 0;

        foreach (DirectoryInfo zigBin in new DirectoryInfo(_config.BinaryFolder).GetDirectories("*"))
        {
            if (!pathFound)
            {
                LocateZigPath(zigBin, out pathFound);
                _index = index;
            }

            Console.WriteLine($" {(_currentBinary is not null && zigBin.FullName == _currentBinary.FullName ? ">>" : "  ")} {zigBin.FullName}");
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
        const char SEPARATOR = ';';
        string? paths = Environment.GetEnvironmentVariable(PATH);
        if (paths is null)
            return [];

        return paths.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries).Select(x => new DirectoryInfo(x)).ToArray();
    }

    public void UpdatePath(DirectoryInfo newZigPath)
    {
        if (!newZigPath.Exists)
            throw new DirectoryNotFoundException("Selected path does not exist");

        StringBuilder newPath = new();

        for (int i = 0; i < _paths.Length; i++)
        {
            if (i == _zigIndex)
            {
                newPath.Append($"{newZigPath.FullName};");
                continue;
            }

            newPath.Append($"{_paths[i].FullName.Replace("\\", "\\\\")};");
        }

        Environment.SetEnvironmentVariable("PATH", newPath.ToString()[..^1]); // ta bort sista ;


    }
}
