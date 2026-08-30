using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RDCore.SDK.Client;
using RDCore.SDK.Platform;
using RDCore.SDK.Server;
using RDCore.SDK.Server.Configuration;
using RDCore.SDK.Server.Services;
using System.IO.Abstractions;

namespace RDCore.LanguageServer;

internal class RDCoreServerProxyFactory(
    IOptions<SdkAppOptions> options,
    IRDCoreServerProcess serverProcess,
    IFileSystem fileSystem,
    IHealthCheckService<RDCoreServerProxy> healthCheckService,
    ILanguageServerProtocolTransportLayer transportLayer,
    ILogger<RDCoreServerProxy> logger) : IRDCoreServerProxyFactory
{
    public RDCoreServerProxy Create(CoreServerComponent platformComponent, CorePlatformClientCapabilities capabilities,
        Action<IRDCoreLSPHandlerConfigurationBuilder>? configureHandlers = default,
        Action<IServiceCollection>? configureServices = default)
        => new(options, platformComponent, capabilities, configureHandlers ?? (builder => { }), configureServices ?? (services => { }),
            serverProcess, fileSystem, healthCheckService, transportLayer, logger);
}
