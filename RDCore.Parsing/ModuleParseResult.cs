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
        SyntaxErrors = [VBSyntaxErrorInfo.For(VBCompileErrorId.SyntaxError, location, verbose)] 
    };

    public ModuleNode? SyntaxTree { get; init; }
    public ImmutableArray<PrecompilerTriviaNode> PrecompilerTrivia { get; init; }
    public ImmutableArray<VBSyntaxErrorInfo> SyntaxErrors { get; init; } = [];

    public bool IsSuccess => SyntaxTree is not null && SyntaxErrors.Length == 0;
}
