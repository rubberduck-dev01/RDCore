using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using RDCore.SDK.Model.Errors;

namespace RDCore.Parsing.PreProcessing;

internal class SllPredictionFailException() : Exception();

internal abstract class TokenStreamParserBase<TParser> : ITokenStreamParser
    where TParser : Parser
{
    protected IParseTree Parse(ITokenStream tokenStream, PredictionMode predictionMode, IEnumerable<IParseTreeListener>? parseListeners = null)
    {
        var parser = GetParser(tokenStream);
        parser.Interpreter.PredictionMode = predictionMode;
        //if (errorListener != null)
        //{
        //    parser.AddErrorListener(errorListener);
        //}
        if (parseListeners != null)
        {
            foreach (var listener in parseListeners)
            {
                parser.AddParseListener(listener);
            }
        }
        return Parse(parser);
    }
    protected abstract TParser GetParser(ITokenStream tokenStream);
    protected abstract IParseTree Parse(TParser parser);
    public IParseTree Parse(Uri uri, CommonTokenStream tokenStream, CancellationToken token, out IEnumerable<VBSyntaxErrorInfo> errors, ParserMode parserMode = ParserMode.FallBackSllToLl, IEnumerable<IParseTreeListener>? parseListeners = null)
    {
        errors = [];
        return parserMode switch
        {
            ParserMode.FallBackSllToLl => ParseWithFallBack(uri, tokenStream, parseListeners),
            ParserMode.LlOnly => ParseLl(uri, tokenStream, parseListeners),
            ParserMode.SllOnly => ParseSll(uri, tokenStream, parseListeners),
            _ => throw new ArgumentException($"Value '{parserMode}' is not supported.", nameof(parserMode)),
        };
    }

    private IParseTree ParseWithFallBack(Uri uri, CommonTokenStream tokenStream, IEnumerable<IParseTreeListener>? parseListeners = null)
    {
        try
        {
            return ParseSll(uri, tokenStream, parseListeners);
        }
        catch (SllPredictionFailException exception)
        {
            LogAndReset(tokenStream, "Parsing (SLL) failed.", exception);
            return ParseLl(uri, tokenStream, parseListeners);
        }
        catch (Exception exception)
        {
            var message = $"SLL mode failed while parsing document (URI {uri}). Retrying using LL prediction mode, but this would be a grammar bug, or a legitimate syntax error.";

            LogAndReset(tokenStream, message, exception);
            return ParseLl(uri, tokenStream, parseListeners);
        }
    }

    //This method is virtual only because a CommonTokenStream cannot be mocked in tests
    //and there is no interface for it or a base class that has the Reset member.
    protected virtual void LogAndReset(CommonTokenStream tokenStream, string logWarnMessage, Exception exception)
    {
        //LogWarning(logWarnMessage, verbose: exception.Message);
        tokenStream.Reset();
    }

    private IParseTree ParseLl(Uri uri, ITokenStream tokenStream, IEnumerable<IParseTreeListener>? parseListeners = null)
    {
        return Parse(tokenStream, PredictionMode.Ll, parseListeners);
    }

    private IParseTree ParseSll(Uri uri, ITokenStream tokenStream, IEnumerable<IParseTreeListener>? parseListeners = null)
    {
        return Parse(tokenStream, PredictionMode.Sll, parseListeners);
    }
}
