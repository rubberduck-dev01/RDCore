using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using RDCore.SDK.Client;
using RDCore.SDK.Extensibility;
using RDCore.SDK.Platform;
using RDCore.SDK.Server;
using RDCore.SDK.Server.Services;
using RDCore.SDK.Server.Services.States;

namespace RDCore.LanguageServer;

/// <summary>
/// The RDCore <strong>RD-VBA Language Server</strong> application.
/// </summary>
/// <remarks>
/// 👉 This application implements a <em>Language Server Protocol (LSP)</em> <strong>server</strong> and is responsible for 
/// <strong>orchestrating communications</strong> between the IDE editor and the applications and services of the RDCore platform.
/// </remarks>
internal sealed class CoreLanguageServerApp(
    IServerStateProvider serverStateProvider,
    IPlatformCompositionService composition,
    IPlatformOrchestrationService orchestration,
    IExtensionsProvider extensionsProvider,
    IHealthCheckService<CoreLanguageServerApp> healthCheckService,
    ILanguageServerProtocolTransportLayer transportLayer,
    ILogger<CoreLanguageServerApp> logger)
    : RDCoreServerApp(serverStateProvider, healthCheckService, transportLayer, logger)
{
    public override CoreServerComponent PlatformComponent => CoreServerComponent.LanguageServer;

    protected override async Task BeforeRunAsync(string[] args)
    {
        var platform = composition.GetManifest();
        LogIfEnabled(LogLevel.Information, "✅ Acquired platform manifest");

        orchestration.RegisterCoreComponent(factory =>
            factory.Create(CoreServerComponent.ParsingServer,
                new CorePlatformClientCapabilities
                {
                    Parsing = new ParserCapabilities
                    {
                        ParseFullDocument = new ParseFullDocument(true)
                    }
                }));
        //.RegisterCoreComponent(factory => factory.Create(CoreServerComponent.EnvironmentHost, TODO));

        LogIfEnabled(LogLevel.Information, "✅ Registered RDCore platform components");

        var loadExtensionTasks = extensionsProvider.Discover().Select(extension => Task.Run(() =>
        {
            orchestration.RegisterExtension(extension, factory => factory.Create(CoreServerComponent.Extension,
                new() /*TODO provide the extension capabilities here*/));
        }));
        await Task.WhenAll(loadExtensionTasks);
        LogIfEnabled(LogLevel.Information, "✅ Registered RDCore platform extensions");
    }

    protected override void ConfigureHandlers(IRDCoreLSPHandlerConfigurationBuilder builder)
    {
        // TODO configure Client <=> LangServer handlers here
    }

    protected override void RegisterServerCapabilities(ILanguageServer server, ClientCapabilities clientCapabilities)
    {
        clientCapabilities.TextDocument = new()
        {
            //CallHierarchy = new(true),
            //CodeAction = new(true),
            //CodeLens = new(true),
            //ColorProvider = new(true),
            //Completion = new(true),
            Declaration = new(true),
            Definition = new(true),
            Diagnostic = new(true),
            //DocumentHighlight = new(true),
            //DocumentLink = new(true),
            DocumentSymbol = new(true),
            //FoldingRange = new(true),
            //Formatting = new(true),
            //Hover = new(true),
            //Implementation = new(true),
            //InlayHint = new(true),
            //InlineValue = new(true),
            //LinkedEditingRange = new(true),
            //Moniker = new(true),
            //OnTypeFormatting = new(true),
            //RangeFormatting = new(true),
            //References = new(true),
            //Rename = new(true),
            //SemanticTokens = new(true),
            //SignatureHelp = new(true),
            //SelectionRange = new(true),
            Synchronization = new(true),
            PublishDiagnostics = new(true),
            //TypeDefinition = new(true),
            //TypeHierarchy = new(true),
        };
        clientCapabilities.Window = new()
        {
            //ShowDocument = new(true),
            ShowMessage = new(true),
            WorkDoneProgress = new(true),
        };
        clientCapabilities.Workspace = new()
        {
            //ApplyEdit = new(true),
            Diagnostics = new(true),
            //FileOperations = new(true),
            //SemanticTokens = new(true),
            Symbol = new(true),
            //WorkspaceEdit = new(true),
            //WorkspaceFolders = new(true),
        };
    }

    protected override void OnLanguageServerStarted(ILanguageServer server)
    {
        LogIfEnabled(LogLevel.Information, "Language Server app started.");
        // TODO some ParsingClientService should be responsible for caching ASTs.
        //var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        ////var uri = new Uri(RDCoreUriNamespaces.RDCoreWorkspaceUri + "/dev-temp1/module1.bas");
        //var request = new ParseDocumentParams
        //{
        //    ModuleType = ModuleType.StdModule,
        //    DocumentUri = uri
        //};
        //_ = orchestration.ParsingService.SendRequestAsync<ParseDocumentParams, ModuleParseResult>(request, tokenSource.Token);
    }

    protected override void Dispose(bool disposing) { }
}
