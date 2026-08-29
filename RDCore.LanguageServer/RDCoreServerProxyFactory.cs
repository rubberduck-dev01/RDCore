using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RDCore.SDK.Client;
using RDCore.SDK.Platform;
using RDCore.SDK.Server;
using RDCore.SDK.Server.Services;
using System.IO.Abstractions;

namespace RDCore.LanguageServer;

internal class RDCoreServerProxyFactory(
    IRDCoreServerProcess serverProcess,
    IFileSystem fileSystem,
    IHealthCheckService<RDCoreServerProxy> healthCheckService,
    ILanguageServerProtocolTransportLayer transportLayer,
    ILogger<RDCoreServerProxy> logger) : IRDCoreServerProxyFactory
{
    public RDCoreServerProxy Create(CoreServerComponent platformComponent, CorePlatformClientCapabilities capabilities,
        Action<IRDCoreLSPHandlerConfigurationBuilder>? configureHandlers = default,
        Action<IServiceCollection>? configureServices = default)
        => new(platformComponent, capabilities, configureHandlers ?? (builder => { }), configureServices ?? (services => { }),
            serverProcess, fileSystem, healthCheckService, transportLayer, logger);
}
