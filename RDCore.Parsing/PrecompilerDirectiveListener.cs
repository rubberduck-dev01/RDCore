using Antlr4.Runtime.Misc;
using RDCore.Parsing.AST;
using RDCore.Parsing.Syntax;
using RDCore.SDK.Model;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.AST.Statements;
using System.Collections.Immutable;

namespace RDCore.Parsing;

internal class PrecompilerNodeBuilder(Uri rootUri, SyntaxNodeId nodeId) : NodeBuilder(rootUri, nodeId)
{
    public SyntaxNode BuildPrecompilerConstDeclaration(VBAConditionalCompilationParser.CcConstContext context, ConstKind kind, AccessModifier modifier)
    {
        var name = context.ccVarLhs().name().nameValue().IDENTIFIER().Symbol.Text;

        return new PrecompilerConstantDeclarationNode(
            NodeId,
            context.GetSourceLocation(_rootUri),
            name,
            kind,
            [.. _children],
            modifier);
    }

    public SyntaxNode BuildConditionalExpression(VBAConditionalCompilationParser.CcExpressionContext context)
        => new ConditionalExpressionNode(NodeId, context.GetSourceLocation(_rootUri), [.. _children]);

    public SyntaxNode BuildPrecompilerConditional(VBAConditionalCompilationParser.CcIfBlockContext context)
        => new PrecompilerIfBlockStatementNode(NodeId, context.GetSourceLocation(_rootUri), [.. _children]);

    public SyntaxNode BuildPrecompilerConditional(VBAConditionalCompilationParser.CcIfContext context)
        => new PrecompilerInlineIfStatementNode(NodeId, context.GetSourceLocation(_rootUri), [.. _children]);
}

internal class PrecompilerDirectiveListener(Uri sourceUri) : VBAConditionalCompilationBaseListener, ISyntaxNodeProvider
{
    private readonly Uri _rootUri = sourceUri;
    private readonly Stack<PrecompilerNodeBuilder> _builderStack = new([new(sourceUri, new($"{sourceUri}/trivia/cc", []))]);

    private readonly List<SyntaxNode> _nodes = [];
    private PrecompilerNodeBuilder CurrentBuilder => _builderStack.Peek();

    private SyntaxNodeId GetCurrentNodeId() => CurrentBuilder.NodeId.Add(CurrentBuilder.ChildCount);

    public ImmutableArray<SyntaxNode> SyntaxNodes => [.. _nodes];

    private void OnEnterParent() => _builderStack.Push(new(_rootUri, GetCurrentNodeId()));
    private void OnExitParent(Func<PrecompilerNodeBuilder, SyntaxNode> provider)
    {
        var node = provider.Invoke(_builderStack.Pop());
        CurrentBuilder.AddChild(node);
    }

    /* NOTE: Conditional compilation nodes must exist in the AST at their source location.
     * The final AST must include a copy of each trivia node with an ID that correctly positions it in the tree structure.
     * Because neither conditionally-compiled branch is semantically meaningful at this layer, no information is collected about here them.
     * The declaration pass can then easily identify AST nodes enclosed within a conditional compilation block.
     * 
     * Eventually the PrecompilerTriviaNode would be replaced with a conditional-compilation expression subtree that the semantic layer can evaluate.
    */ 

    public override void EnterCcBlock([NotNull] VBAConditionalCompilationParser.CcBlockContext context) 
        => OnEnterParent();
    public override void ExitCcBlock([NotNull] VBAConditionalCompilationParser.CcBlockContext context) 
        => OnExitParent(provider => new PrecompilerTriviaNode(GetCurrentNodeId(), context.GetSourceLocation(_rootUri), [], context.GetText()));
}
