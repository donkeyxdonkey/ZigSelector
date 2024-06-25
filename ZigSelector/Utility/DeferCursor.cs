namespace ZigSelector.Utility;

internal class DeferCursor : IDisposable
{
    private DeferCursor()
    {
        Console.CursorVisible = false;
        Console.Clear();
    }

    public static DeferCursor Defer() => new();

    public void Dispose()
    {
        Console.CursorVisible = true;
        Console.Clear();
        GC.SuppressFinalize(this);
    }
}
