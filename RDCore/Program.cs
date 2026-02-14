using RDCore.Configuration;
using RDCore.Server;

namespace RDCore;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var options = Args.Parse(args);
            using var app = new ServerApp(options);
            await app.RunAsync();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return -1;
        }

        return 0;
    }
}
