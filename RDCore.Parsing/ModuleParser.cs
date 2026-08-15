using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using RDCore.Parsing.AST;
using RDCore.Parsing.PreProcessing;
using RDCore.Parsing.Syntax;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.Errors;
using RDCore.SDK.Model.Source;
using System.Collections.Immutable;

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
            var node = new ModuleNode(new SyntaxNodeId(uri.AbsolutePath, []), new(uri, SourceRange.Empty), [], moduleType);
            var listener = new DeclarationsParseTreeListener(uri, node);
            var errorListener = new ErrorListener(uri);
            try
            {
                ParseWithFallback(tokenStream, errorListener, [listener]);

                var ast = listener.BuildModuleNode();
                return ast.Children.Length > 0 
                    ? ModuleParseResult.Success(ast) with { SyntaxError = errorListener.Errors.FirstOrDefault() }
                    : ModuleParseResult.Failed(node.SourceLocation, errorListener.Errors.FirstOrDefault()?.Verbose ?? string.Empty);
                ;
            }
            catch (Exception exception)
            {
                var verbose = $"Parsing failed: {exception}";
                return ModuleParseResult.Failed(new(uri, SourceRange.Empty), verbose);
            }
        }
        var verbosePreprocessorFailed = "Preprocessing failed";
        return ModuleParseResult.Failed(new(uri, SourceRange.Empty), verbosePreprocessorFailed);
    }

    private static void ParseWithFallback(CommonTokenStream tokenStream, ErrorListener errorListener, IParseTreeListener[] listeners)
    {
        try
        {
            ParseFast(tokenStream, errorListener, listeners);
        }
        catch (InputMismatchException)
        {
            ParseSlow(tokenStream, errorListener, listeners);
        }
        catch (RecognitionException)
        {
            ParseSlow(tokenStream, errorListener, listeners);
        }
    }

    private static void ParseFast(CommonTokenStream tokenStream, ErrorListener errorListener, IParseTreeListener[] listeners) 
        => Parse(tokenStream, PredictionMode.Sll, errorListener, listeners);

    private static void ParseSlow(CommonTokenStream tokenStream, ErrorListener errorListener, IParseTreeListener[] listeners) 
        => Parse(tokenStream, PredictionMode.Ll, errorListener, listeners);

    private static void Parse(CommonTokenStream tokenStream, PredictionMode mode, ErrorListener errorListener, IParseTreeListener[] listeners)
    {
        var parser = new VBAParser(tokenStream);
        parser.Interpreter.PredictionMode = mode;
        parser.AddErrorListener(errorListener);
        foreach (var listener in listeners)
        {
            parser.AddParseListener(listener);
        }
        parser.startRule();
    }
}

internal class ErrorListener(Uri uri) : IAntlrErrorListener<IToken>
{
    private readonly Uri _uri = uri;
    private readonly List<VBSyntaxErrorInfo> _errors = [];
    public ImmutableArray<VBSyntaxErrorInfo> Errors => [.. _errors];

    public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
    {
        var location = new SourceLocation(_uri, new(line, charPositionInLine, line, charPositionInLine));
        _errors.Add(VBSyntaxErrorInfo.For(VBCompileErrorId.SyntaxError, location, msg));
    }
}