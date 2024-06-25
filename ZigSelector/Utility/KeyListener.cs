using ZigSelector.Data;

namespace ZigSelector.Utility;

internal class KeyListener
{
    private readonly ConsoleKey[] _allowedKeys;

    public KeyListener()
    {
        _allowedKeys = ApplicationData.AllowedKeys;
    }

    public ConsoleKey Listen() => FindKeyInArray();

    private ConsoleKey FindKeyInArray()
    {
        ConsoleKeyInfo keyInfo;
        do
        {
            keyInfo = Console.ReadKey(intercept: true);
        } while (!Array.Exists(_allowedKeys, x => x == keyInfo.Key));

        return keyInfo.Key;
    }
}
