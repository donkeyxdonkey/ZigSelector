using System.Text;
using ZigSelector.ConfigModel;
using ZigSelector.Utility;

namespace ZigSelector;

public class VersionSelector
{
    const string PATH = "PATH";

    public State State { get => _state; }

    private readonly State _state;
    private readonly DirectoryInfo[] _paths;
    private readonly Configuration _config;
    private readonly short _zigIndex;

    public VersionSelector()
    {
        _paths = GeneratePaths();
        _state = _paths.Length != 0 ? State.Active : State.Terminate;
        _config = Helper.GetConfiguration();
        _zigIndex = -1;

        //UpdatePath(new DirectoryInfo("korv"));
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

        ;
        Environment.SetEnvironmentVariable("PATH", newPath.ToString()[..^1]); // ta bort sista ;
    }
}
