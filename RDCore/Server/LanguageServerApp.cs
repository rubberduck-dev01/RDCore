using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Server;
using RDCore.Server.Handlers.Document;
using RDCore.Server.Handlers.Lifecycle;
using RDCore.Server.Handlers.Workspace;
using RDCore.Server.Services;
using RDCore.Server.States;
using RDCore.Workspace.Services;
using System.IO.Pipelines;
using System.IO.Pipes;
using OmniSharpLanguageServer = OmniSharp.Extensions.LanguageServer.Server.LanguageServer;

namespace RDCore.Server;

internal interface ILanguageServerApp : IDisposable
{
    ILanguageServer LanguageServer { get; }
    Task RunAsync(IServiceProvider provider);
}

internal class LanguageServerApp(
    IServerStateProvider serverStateProvider,
    IHealthCheckService healthCheckService,
    IWorkspaceService workspaceService,
    ILogger<LanguageServerApp> logger) : ILanguageServerApp
{
    private static string PipeName => "RDCore.Server";

    private OmniSharpLanguageServer? Server { get; set; } = default;

    private NamedPipeServerStream NamedPipeServerStream { get; set; } = default!;
    private Task? WaitForClientConnectionTask { get; set; }

    public ILanguageServer LanguageServer => Server ?? throw new InvalidOperationException("Language server has not been initialized.");

    public async Task RunAsync(IServiceProvider provider)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogInformation("Starting language server...");
        }
        Server = await OmniSharpLanguageServer.From(ConfigureServer, provider, serverStateProvider.ProcessToken);

        logger.LogInformation("✅ Language server is ready and awaiting client initialize request.");
        await Server.WaitForExit;

        logger.LogInformation("✅ RunAsync completed; process will now exit.");
    }

    public void Dispose()
    {
        if (NamedPipeServerStream is IDisposable disposableStream)
        {
            disposableStream.Dispose();
        }
        if (WaitForClientConnectionTask is IDisposable disposableTask)
        {
            disposableTask.Dispose();
        }
        if (Server is IDisposable disposableServer)
        {
            disposableServer.Dispose();
        }
    }

    private void ConfigureServer(LanguageServerOptions options)
    {
        NamedPipeServerStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut,
            serverStateProvider.Options.MaximumInstances,
            PipeTransmissionMode.Byte,
            System.IO.Pipes.PipeOptions.Asynchronous |
            System.IO.Pipes.PipeOptions.CurrentUserOnly);

        options
            .WithInput(PipeReader.Create(NamedPipeServerStream))
            .WithOutput(PipeWriter.Create(NamedPipeServerStream));

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Named pipe '{pipeName}' initialized; asynchronously awaiting client connection...", PipeName);
        }
        WaitForClientConnectionTask = NamedPipeServerStream.WaitForConnectionAsync(serverStateProvider.ProcessToken);

        options
            .WithServerInfo(serverStateProvider.ServerInfo)
            .WithHandler<ExitHandler>()
            .WithHandler<ShutdownHandler>()
            .WithHandler<SetTraceHandler>()
            .WithHandler<DidOpenTextDocumentHandler>()
            .WithHandler<DidCloseTextDocumentHandler>()
            .WithHandler<DidChangeTextDocumentHandler>()
            .WithHandler<ExecuteCommandHandler>()
            .OnStarted(OnLanguageServerStartedAsync)
            .OnInitialize(OnLanguageServerInitializeAsync)
            .OnInitialized(OnLanguageServerInitializedAsync)
            ;

        options.WithServices(services =>
        {
            services.AddLogging(builder =>
            {
                builder.AddLanguageProtocolLogging();
            });
        });

        logger.LogInformation("✅ ConfigureServer completed.");
    }

    private async Task OnLanguageServerInitializeAsync(ILanguageServer server, InitializeParams request, CancellationToken token)
    {
        // ## OnLanguageServerInitializeDelegate
        // Gives your class or handler an opportunity to interact with the InitializeParams
        // before it is processed by the server.
        serverStateProvider.OnInitialize();

        if (request.ProcessId.HasValue)
        {
            // Initialize request specifies a client process ID; start monitoring the client process health
            healthCheckService.Start(request.ProcessId);
        }
        else
        {
            logger.LogWarning("Initialize request did not specify a client process ID. Skipping client process health checks; this server instance will not automatically exit.");
        }

        // TODO - initialize server capabilities based on the workspace and client capabilities specified in the request

        if ((request.RootUri?.GetFileSystemPath() ?? request.RootPath) is string workspaceRoot)
        {
            await workspaceService.LoadAsync(workspaceRoot);
        }
        else
        {
            logger.LogWarning("⚠️ Initialize request did not specify a workspace root path or URI; an exception will be thrown.");
            throw InvalidRequestException.For(request);
        }

        logger.LogInformation("✅ OnLanguageServerInitializeAsync handler completed.");
    }

    private async Task OnLanguageServerInitializedAsync(ILanguageServer server, InitializeParams request, InitializeResult response, CancellationToken cancellationToken)
    {
        // ## OnLanguageServerInitializedDelegate
        // Gives your class or handler an opportunity to interact with the InitializeParams and InitializeResult
        // after it is processed by the server but before it is sent to the client.
        serverStateProvider.OnInitialized();
        logger.LogInformation("✅ OnLanguageServerInitializedAsync handler completed.");
    }

    private async Task OnLanguageServerStartedAsync(ILanguageServer server, CancellationToken token)
    {
        // ## OnLanguageServerStartedDelegate
        // Gives your class or handler an opportunity to interact with the ILanguageServer
        // after the connection has been established.

        // TODO - start parsing, we're ready to receive client requests and notifications at this point.
        logger.LogInformation("✅ OnLanguageServerStartedAsync handler completed.");
    }
}
