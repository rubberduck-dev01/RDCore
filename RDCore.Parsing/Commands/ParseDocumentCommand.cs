using RDCore.SDK.Server.Commands;
using RDCore.SDK.Server.Commands.Parsing;

namespace RDCore.Parsing.Commands;

/// <summary>
/// Commands the <c>RDCore.Parser</c> server to parse the document at a <c>Uri</c>.
/// </summary>
/// <param name="argsParser">A service that parses the raw <c>JToken</c> parameters.</param>
public sealed class ParseDocumentCommand(ICommandParamsParser<ParseDocumentParams> argsParser) 
    : ServerCommand<ParseDocumentParams>(argsParser, SdkServerCommandNames.ParseDocument)
{
    protected override async Task ExecuteAsync(ParseDocumentParams? args, CancellationToken token)
    {
        if (args?.DocumentUri is Uri uri)
        {
            // TODO full-document parse
        }
    }
}
