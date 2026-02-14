using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using RDCore.Server.States;

namespace RDCore.Server.Handlers.Lifecycle;

internal class SetTraceHandler(ILogger<IJsonRpcHandler> logger, IServerStateProvider server) : SetTraceHandlerBase
{
    public async override Task<Unit> Handle(SetTraceParams request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogTrace("Received SetTrace request.");

        // TODO
        
        return await Task.FromResult(Unit.Value);
    }
}