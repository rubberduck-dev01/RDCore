using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using RDCore.SDK.Server;
using RDCore.SDK.Server.Services;
using RDCore.SDK.Server.Services.States;

namespace RDCore.Parsing;

internal class RDCoreExtensionServerApp(
    IServerStateProvider serverStateProvider,
    IHealthCheckService<ILanguageServerApp> healthCheckService,
    ILanguageServerProtocolTransportLayer transportLayer,
    ILogger<RDCoreExtensionServerApp> logger)
    : LanguageServerApp(serverStateProvider, healthCheckService, transportLayer, logger)
{
    protected override void ConfigureHandlers(IRDCoreLSPHandlerConfigurationBuilder builder)
    {
        // TODO
    }

    protected override async Task RegisterServerCapabilitiesAsync(ILanguageServer server, ClientCapabilities clientCapabilities, CancellationToken token)
    {
        // TODO
    }

    protected override void Dispose(bool disposing) { }
}
