using RDCore.Configuration;
using RDCore.Server;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("RDCore.Tests")]

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
