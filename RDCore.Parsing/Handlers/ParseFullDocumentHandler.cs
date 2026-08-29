using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.JsonRpc.Server;
using RDCore.SDK.Client;
using RDCore.SDK.Model.AST;
using RDCore.SDK.Platform.Protocol;
using System.IO.Abstractions;

namespace RDCore.Parsing.Handlers;

[Method(RDCorePlatformProtocol.ParseFullDocument)]
internal class ParseFullDocumentHandler(IFile fileService, IModuleParser moduleParser)
    : RDCoreRequestHandler<ParseDocumentParams, ModuleParseResult>
{
    protected override async Task<ModuleParseResult> HandleAsync(ParseDocumentParams request, CancellationToken token)
    {
        if (request?.DocumentUri is Uri uri)
        {
            var content = fileService.ReadAllText(uri.AbsolutePath);
            return moduleParser.Parse(uri, request.ModuleType, content);
        }

        throw new InvalidParametersException(request);
    }
}
