using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using RDCore.Parsing.AST;
using RDCore.Parsing.PreProcessing;
using RDCore.Parsing.Syntax;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.Errors;
using RDCore.SDK.Model.Source;

namespace RDCore.Parsing;

internal record class ModuleParseResult
{
    public static ModuleParseResult Success(ModuleNode node) => new() { SyntaxTree = node };
    public static ModuleParseResult Failed(SourceLocation location, string verbose) => new() 
    {
        SyntaxError = VBSyntaxErrorInfo.For(VBCompileErrorId.SyntaxError, location, verbose) 
    };

    public ModuleNode? SyntaxTree { get; init; }
    public VBSyntaxErrorInfo? SyntaxError { get; init; }

    public bool IsSuccess => SyntaxTree is not null && SyntaxError is null;
}

internal interface IModuleParser
{
    ModuleParseResult Parse(Uri uri, ModuleType moduleType, Stream content);
}

internal class ModuleParser(ITokenStreamPreprocessor preprocessor) : IModuleParser
{
    public ModuleParseResult Parse(Uri uri, ModuleType moduleType, Stream content)
    {
        var input = new AntlrInputStream(content);
        var lexer = new VBALexer(input);
        var rawTokenStream = new CommonTokenStream(lexer);

        if (preprocessor.PreprocessTokenStream(uri, rawTokenStream) is CommonTokenStream tokenStream)
        {
            var node = new ModuleNode(uri, new(uri, SourceRange.Empty), [], moduleType);
            var listener = new DeclarationsParseTreeListener(node);

            try
            {
                ParseWithFallback(tokenStream, [listener]);

                var ast = listener.BuildModuleNode();
                return ModuleParseResult.Success(ast);
            }
            catch (RecognitionException exception)
            {
                var token = exception.OffendingToken;
                return ModuleParseResult.Failed(new(uri, new(token.Line, token.Column, token.Line, token.Column)),
                    exception.Message);
            }
            catch
            {
                var verbose = "Parsing failed";
                return ModuleParseResult.Failed(new(uri, SourceRange.Empty), verbose);
            }
        }
        var verbosePreprocessorFailed = "Preprocessing failed";
        return ModuleParseResult.Failed(new(uri, SourceRange.Empty), verbosePreprocessorFailed);
    }

    private static void ParseWithFallback(CommonTokenStream tokenStream, IParseTreeListener[] listeners)
    {
        try
        {
            ParseFast(tokenStream, listeners);
        }
        catch
        {
            ParseSlow(tokenStream, listeners);
        }
    }

    private static void ParseFast(CommonTokenStream tokenStream, IParseTreeListener[] listeners) 
        => Parse(tokenStream, PredictionMode.Sll, listeners);

    private static void ParseSlow(CommonTokenStream tokenStream, IParseTreeListener[] listeners) 
        => Parse(tokenStream, PredictionMode.Ll, listeners);

    private static void Parse(CommonTokenStream tokenStream, PredictionMode mode, IParseTreeListener[] listeners)
    {
        var parser = new VBAParser(tokenStream);
        parser.Interpreter.PredictionMode = mode;
        
        foreach (var listener in listeners)
        {
            parser.AddParseListener(listener);
        }
        parser.startRule();
    }
}
