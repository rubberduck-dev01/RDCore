using Newtonsoft.Json.Linq;
using RDCore.SDK.Model.Source;
using RDCore.SDK.Server.Commands;
using RDCore.SDK.Server.Commands.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace RDCore.Parsing.Commands;

internal class ParseDocumentParamsParser : ICommandParamsParser<ParseDocumentParams>
{
    private static bool TryParseDocumentUri(JToken arg, [MaybeNullWhen(false)][NotNullWhen(true)] out Uri? uri) 
        => Uri.TryCreate(arg.Value<string>(), UriKind.Absolute, out uri);

    private static bool TryParseFragment(JToken arg, [MaybeNullWhen(false)][NotNullWhen(true)] out string? fragment)
    {
        fragment = arg.Value<string>();
        return fragment is not null;
    }
    private static bool TryParseAnchorOffset(JToken arg, [MaybeNullWhen(false)][NotNullWhen(true)] out SourcePosition? offset)
    {
        offset = arg.Value<SourcePosition>();
        return offset is not null;
    }

    public ParseDocumentParams? Parse(JArray? args)
    {
        if (args is JArray arr)
        {
            switch (arr.Count)
            {
                case 1:
                    {
                        if (TryParseDocumentUri(args[0], out var uri))
                        {
                            return new() { DocumentUri = uri };
                        }
                        else if (TryParseFragment(args[0], out var fragment))
                        {
                            // NOTE: anchor offset is implicitly SourcePosition.Zero in this case.
                            return new() { Fragment = fragment };
                        }
                        break;
                    }
                case 2:
                    {
                        var offset = SourcePosition.Zero;
                        if (TryParseAnchorOffset(args[1], out var anchor))
                        {
                            offset = anchor.Value;
                        }
                        if (TryParseDocumentUri(args[0], out var uri))
                        {
                            return new() 
                            {
                                DocumentUri = uri,
                                AnchorOffset = offset
                            };
                        }
                        else if (TryParseFragment(args[0], out var fragment))
                        {
                            return new()
                            {
                                Fragment = fragment,
                                AnchorOffset = offset
                            };
                        }
                        break;
                    }
            }
        }
        return null;
    }
}
