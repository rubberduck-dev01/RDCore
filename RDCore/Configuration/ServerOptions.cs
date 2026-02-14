using CommandLine;

namespace RDCore.Configuration;

public record class ServerOptions
{
    private const int DefaultHealthCheckIntervalSeconds = 5;

    [Option('i', "healthcheck-interval", Required = false, Default = DefaultHealthCheckIntervalSeconds, HelpText = "Interval (in seconds) between client process health checks.")]
    public int HealthCheckIntervalSeconds { get; init; } = DefaultHealthCheckIntervalSeconds;

    [Option('v', "verbose", Required = false, HelpText = "Enable verbose output. Can be changed later by sending a SetTrace request from the client process.")]
    public bool Verbose { get; init; } = false;
}
