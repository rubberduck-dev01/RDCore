using RDCore.Configuration;
using RDCore.Server;
using RDCore.Server.States;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("RDCore.Tests")]

namespace RDCore;

public class Program
{
    private static readonly ServerStateProvider _stateProvider = new();

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var options = Args.Parse(args);

            using var app = new ServerApp(_stateProvider, options);
            await app.RunAsync();
        }
        catch (Exception exception)
        {
            // logger is disposed at this point
            Console.WriteLine(exception);
        }

        return _stateProvider.State.ExitCode;
    }
}
