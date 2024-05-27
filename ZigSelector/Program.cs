using ZigSelector;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            VersionSelector zigSelector = new();

            while (zigSelector.State == State.Active)
            {

                break;
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception)
        {

            throw;
        }
    }
}