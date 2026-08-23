using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using RDCore.SDK.Client;
using RDCore.SDK.Server;
using RDCore.SDK.Server.Services;
using RDCore.SDK.Server.Services.States;

namespace RDCore.Diagnostics;

internal class CoreDiagnosticsAppHost() : RDCorePlatformServerHost<CoreDiagnosticsApp>()
{
    protected override void ConfigureAdditionalExternalServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}


internal class CoreDiagnosticsApp : RDCoreServerApp
{
    public CoreDiagnosticsApp(
        //IOptions<SdkServerOptions> options, 
        IServerStateProvider serverStateProvider, 
        IHealthCheckService<CoreDiagnosticsApp> healthCheckService, 
        ILanguageServerProtocolTransportLayer transportLayer, 
        ILogger<CoreDiagnosticsApp> logger) 
        : base(serverStateProvider, healthCheckService, transportLayer, logger)
    {
    }

    public override CoreServerComponent PlatformComponent => CoreServerComponent.Extension;

    protected override void ConfigureHandlers(IRDCoreLSPHandlerConfigurationBuilder builder)
    {
    }

    protected override void Dispose(bool disposing)
    {
    }

    protected override void RegisterServerCapabilities(ILanguageServer server, CorePlatformClientCapabilities clientCapabilities)
    {
    }
}