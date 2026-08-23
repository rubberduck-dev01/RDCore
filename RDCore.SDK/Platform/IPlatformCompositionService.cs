using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using RDCore.SDK.Client;
using RDCore.SDK.Extensibility;
using RDCore.SDK.Server;
using RDCore.SDK.Server.Services;
using System.Collections.Immutable;
using System.IO.Abstractions;

namespace RDCore.SDK.Platform;

public interface IPlatformCompositionService
{
    string RootPath { get; }
    PlatformManifest GetManifest();
    ImmutableArray<ExtensionInfo> GetExtensions();
}

/// <summary>
/// An <em>abstract factory</em> that creates clients for platform components.
/// </summary>
public interface IRDCoreServerProxyFactory
{
    RDCoreServerProxy Create(CoreServerComponent platformComponent, CorePlatformClientCapabilities capabilities,
        Action<IRDCoreLSPHandlerConfigurationBuilder>? configureHandlers = default,
        Action<IServiceCollection>? configureServices = default);
}
public class RDCoreServerProxyFactory(
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

public class RDCoreServerProxy : RDCoreClientApp
{
    private readonly CoreServerComponent _platformComponent;
    private readonly CorePlatformClientCapabilities _capabilities;
    private readonly Action<IRDCoreLSPHandlerConfigurationBuilder> _configureHandlers;
    private readonly Action<IServiceCollection> _configureServices;

    public RDCoreServerProxy(
        CoreServerComponent platformComponent, 
        CorePlatformClientCapabilities capabilities,
        Action<IRDCoreLSPHandlerConfigurationBuilder> configureHandlers,
        Action<IServiceCollection> configureServices,
        IRDCoreServerProcess serverProcess, 
        IFileSystem fileSystem, 
        IHealthCheckService<RDCoreClientApp> healthCheckService, 
        ILanguageServerProtocolTransportLayer transportLayer, 
        ILogger<RDCoreClientApp> logger) 
        : base(serverProcess, fileSystem, healthCheckService, transportLayer, logger)
    {
        _platformComponent = platformComponent;
        _capabilities = capabilities;
        _configureHandlers = configureHandlers;
        _configureServices = configureServices;
    }

    public override CoreServerComponent PlatformComponent => _platformComponent;

    protected override ClientCapabilities ConfigureClientCapabilities(ClientCapabilities capabilities)
    {
        capabilities.Experimental = new Dictionary<string, JToken>() { ["rdcore"] = JToken.FromObject(_capabilities) };
        return capabilities;
    }

    protected override void ConfigureHandlers(IRDCoreLSPHandlerConfigurationBuilder builder) => _configureHandlers(builder);

    protected override void ConfigureServices(IServiceCollection services) => _configureServices(services);

    protected override void Dispose(bool disposing) { }
}