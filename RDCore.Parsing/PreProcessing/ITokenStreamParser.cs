using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using RDCore.SDK.Model.Errors;

namespace RDCore.Parsing.PreProcessing;

public enum ParserMode
{
    FallBackSllToLl,
    LlOnly,
    SllOnly
}

public interface ITokenStreamParser
{
    IParseTree Parse(Uri uri, CommonTokenStream tokenStream, 
        CancellationToken token, 
        out IEnumerable<VBSyntaxErrorInfo> errors, 
        ParserMode parserMode = ParserMode.FallBackSllToLl, 
        IEnumerable<IParseTreeListener>? parseListeners = null);
}
