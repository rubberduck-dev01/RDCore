using RDCore.SDK.Server.Commands;
using RDCore.SDK.Server.Commands.Parsing;
using System.IO.Abstractions;

namespace RDCore.Parsing.Commands;

/// <summary>
/// Commands the <c>RDCore.Parser</c> server to parse the document at a <c>Uri</c>.
/// </summary>
/// <param name="argsParser">A service that parses the raw <c>JToken</c> parameters.</param>
/// <param name="moduleParser">A service that parses the content of a source file (module).</param>
internal sealed class ParseDocumentCommand(ICommandParamsParser<ParseDocumentParams> argsParser, 
    IFile fileService,
    IModuleParser moduleParser) 
    : ServerCommand<ParseDocumentParams>(argsParser, SdkServerCommandNames.ParseDocument)
{
    protected override async Task ExecuteAsync(ParseDocumentParams? args, CancellationToken token)
    {
        if (args?.DocumentUri is Uri uri)
        {
            var content = fileService.ReadAllText(uri.AbsolutePath);
            var result = moduleParser.Parse(uri, args.ModuleType, content);
            if (result.IsSuccess)
            {
                
            }
        }
    }
}
