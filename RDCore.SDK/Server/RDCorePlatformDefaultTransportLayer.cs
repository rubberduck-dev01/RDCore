using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Client;
using OmniSharp.Extensions.LanguageServer.Server;
using RDCore.SDK.Server.Configuration;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Reflection.Metadata;

namespace RDCore.SDK.Server;

/// <summary>
/// The RDCore LSP transport layer interface.
/// </summary>
public interface ILanguageServerProtocolTransportLayer
{
    /// <summary>
    /// Configures server-side transport.
    /// </summary>
    NamedPipeServerStream ConfigureServer();
    /// <summary>
    /// Configures client-side transport.
    /// </summary>
    NamedPipeClientStream ConfigureClient(string pipeName);
}


/// <summary>
/// The default <c>RDCore.SDK</c> transport layer implementation.
/// </summary>
/// <remarks>
/// Implements the client/server connection over <em>named pipes</em> streams.
/// </remarks>
public sealed class RDCorePlatformDefaultTransportLayer(IOptions<SdkAppOptions> Options) 
    : ILanguageServerProtocolTransportLayer
{
    public NamedPipeServerStream ConfigureServer() 
        => new(Options.Value.Platform.Transport.PipeConfig.PipeName, PipeDirection.InOut,
            Options.Value.Platform.Transport.PipeConfig.MaximumInstances,
            PipeTransmissionMode.Byte, // NOTE: 'Message' transmission mode is only supported with Windows pipes.
            System.IO.Pipes.PipeOptions.CurrentUserOnly);

    public NamedPipeClientStream ConfigureClient(string pipeName) 
        => new(".", pipeName, PipeDirection.InOut, System.IO.Pipes.PipeOptions.CurrentUserOnly);
}
