using Antlr4.Runtime;
using RDCore.Parsing.PreProcessing.Legacy;
namespace RDCore.Parsing.PreProcessing;

internal interface ICompilationArgumentsProvider
{
    VBAPredefinedCompilationConstants PredefinedCompilationConstants { get; }
    Dictionary<string, short> UserDefinedCompilationArguments(Uri workspaceRoot);
}

internal sealed class VBAPreprocessor : ITokenStreamPreprocessor
{
    private readonly ITokenStreamParser _parser;
    private readonly ICompilationArgumentsProvider _compilationArgumentsProvider;

    public VBAPreprocessor(VBAPreprocessorParser preprocessorParser, ICompilationArgumentsProvider compilationArgumentsProvider)
    {
        _compilationArgumentsProvider = compilationArgumentsProvider;
        _parser = preprocessorParser;
    }

    public CommonTokenStream? PreprocessTokenStream(Uri uri, CommonTokenStream tokenStream)
    {
        var tree = _parser.Parse(uri, tokenStream, out _);
        var charStream = tokenStream.TokenSource.InputStream;
        var symbolTable = new SymbolTable<string, IValue>();
        Dictionary<string, short> userCompilationArguments = [];
        try
        {
            userCompilationArguments = _compilationArgumentsProvider.UserDefinedCompilationArguments(uri) ?? [];
        }
        catch (Exception)
        {
            // could not read precompiler constants from ITypeLib
            // let's still attempt to return a token stream.
        }
        var predefinedCompilationArgument = _compilationArgumentsProvider.PredefinedCompilationConstants; 
        var evaluator = new VBAPreprocessorVisitor(symbolTable, predefinedCompilationArgument, userCompilationArguments, charStream, tokenStream);
        var expr = evaluator.Visit(tree);
        _ = expr.Evaluate(); //This does the actual preprocessing of the token stream as a side effect.
        tokenStream.Reset();
        return tokenStream;
    }
}
