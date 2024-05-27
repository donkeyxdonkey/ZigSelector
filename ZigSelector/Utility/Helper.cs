using Microsoft.Extensions.Configuration;
using ZigSelector.ConfigModel;

namespace ZigSelector.Utility;

public class Helper
{
    public static Configuration GetConfiguration()
    {
        const string CONFIG_FILE = "config.json";

        FileInfo configFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, CONFIG_FILE));

        if (!configFile.Exists)
            throw new FileNotFoundException($"Missing configuration file: {configFile.FullName}.");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(CONFIG_FILE, optional: false, reloadOnChange: true)
            .Build();

        IConfigurationSection bajs = configuration.GetSection("Configuration");
        Configuration config = new();
        bajs.Bind(config);
        return config;
    }
}
