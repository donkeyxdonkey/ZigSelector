using ZigSelector.ConfigModel;
using ZigSelector.Utility;

namespace ZigSelector;

public class VersionSelector
{
    public State State { get => _state; }

    private readonly State _state;
    private readonly DirectoryInfo[] _paths;
    private readonly Configuration _config;

    public VersionSelector()
    {
        _paths = GeneratePaths();
        _state = _paths.Length != 0 ? State.Active : State.Terminate;
        _config = Helper.GetConfiguration();
    }

    private DirectoryInfo[] GeneratePaths()
    {
        const char SEPARATOR = ';';
        string? paths = Environment.GetEnvironmentVariable("PATH");
        if (paths is null)
            return [];

        return paths.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries).Select(x => new DirectoryInfo(x)).ToArray();
    }
}
