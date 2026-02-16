using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RDCore.Server.Commands;
using RDCore.Server.Services;
using RDCore.Server.States;
using RDCore.Workspace.Services;
using RDCore.Workspace.States;
using System.IO.Abstractions;
using System.Reflection;

using IFile = System.IO.Abstractions.IFile;

namespace RDCore.Server;

internal class ServerApp(IServerStateProvider serverStateProvider)
{
    public static AssemblyName Info { get; } = typeof(ServerApp).Assembly.GetName();
    public async Task RunAsync()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        using var provider = services.BuildServiceProvider();
        var app = provider.GetRequiredService<ILanguageServerApp>();
        await app.RunAsync(provider);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<IFileSystem, FileSystem>()
            .AddSingleton<IPath, PathWrapper>()
            .AddSingleton<IFile, FileWrapper>()
            .AddSingleton<IDirectory, DirectoryWrapper>()

            .AddSingleton(provider => Info.Version!)

            .AddSingleton<ILanguageServerApp, LanguageServerApp>()
            .AddSingleton<IServerStateProvider>(serverStateProvider)
            .AddSingleton<IDocumentStateProvider, DocumentStateProvider>()
            .AddSingleton<IHealthCheckService, HealthCheckService>()
            .AddSingleton<IWorkspaceService, WorkspaceService>()
            .AddSingleton<IProjectFileService, ProjectFileService>()
            .AddSingleton<IWorkspaceDocumentService, WorkspaceDocumentService>()

            .AddSingleton<IEnumerable<SupportedLanguage>>(provider => [SupportedLanguage.VBA])
            .AddSingleton<TextDocumentSelector>(provider => SupportedLanguage.VBA.ToTextDocumentSelector())

            .AddSingleton<ServerCommand, AddReferenceCommand>()
            .AddSingleton<ServerCommand, RemoveReferenceCommand>()

            .AddLogging(ConfigureLogging);
    }

    private void ConfigureLogging(ILoggingBuilder builder)
    {
        if (serverStateProvider.Options.Verbose)
        {
            builder.SetMinimumLevel(LogLevel.Trace);
        }
        else
        {
            builder.SetMinimumLevel(LogLevel.Information);
        }

        builder.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "[HH:mm:ss.fff] ";
        });

#if DEBUG
        builder.AddDebug();
#endif
    }
}
