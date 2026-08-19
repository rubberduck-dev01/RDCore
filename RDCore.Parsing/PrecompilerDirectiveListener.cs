using Antlr4.Runtime.Misc;
using RDCore.Parsing.AST;
using RDCore.Parsing.Syntax;
using RDCore.SDK.Model;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.AST.Statements;
using System.Collections.Immutable;
using System.Diagnostics;

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
    private readonly Stack<PrecompilerNodeBuilder> _builderStack = new([new(sourceUri, new($"{sourceUri}/conditional", []))]);

    private PrecompilerNodeBuilder CurrentBuilder => _builderStack.Peek();

    private SyntaxNodeId GetCurrentNodeId() => CurrentBuilder.NodeId.Add(CurrentBuilder.ChildCount);

    public ImmutableArray<SyntaxNode> SyntaxNodes => BuildModuleNode();

    public ImmutableArray<SyntaxNode> BuildModuleNode()
    {
        Debug.Assert(_builderStack.Count == 1);
        return [.. CurrentBuilder.GetChildren];
    }

    private void OnEnterParent() => _builderStack.Push(new(_rootUri, GetCurrentNodeId()));
    private void OnExitParent(Func<PrecompilerNodeBuilder, SyntaxNode> provider)
    {
        var node = provider.Invoke(_builderStack.Pop());
        CurrentBuilder.AddChild(node);
    }

    public override void EnterCcBlock([NotNull] VBAConditionalCompilationParser.CcBlockContext context) 
        => OnEnterParent();
    public override void ExitCcBlock([NotNull] VBAConditionalCompilationParser.CcBlockContext context) 
        => OnExitParent(provider => new PrecompilerTriviaNode(GetCurrentNodeId(), context.GetSourceLocation(_rootUri), [], context.GetText()));

    public override void EnterCcIf([NotNull] VBAConditionalCompilationParser.CcIfContext context)
        => OnEnterParent();
    public override void ExitCcIf([NotNull] VBAConditionalCompilationParser.CcIfContext context)
        => OnExitParent(provider => provider.BuildPrecompilerConditional(context));

    public override void EnterCcIfBlock([NotNull] VBAConditionalCompilationParser.CcIfBlockContext context)
        => OnEnterParent();
    public override void ExitCcIfBlock([NotNull] VBAConditionalCompilationParser.CcIfBlockContext context)
        => OnExitParent(provider => provider.BuildPrecompilerConditional(context));

    public override void EnterCcExpression([NotNull] VBAConditionalCompilationParser.CcExpressionContext context)
        => OnEnterParent();
    public override void ExitCcExpression([NotNull] VBAConditionalCompilationParser.CcExpressionContext context)
        => OnExitParent(provider => provider.BuildConditionalExpression(context));

    public override void ExitCcVarLhs([NotNull] VBAConditionalCompilationParser.CcVarLhsContext context)
        => CurrentBuilder.AddChild(new PrecompilerNameExpressionNode(GetCurrentNodeId(), context.GetSourceLocation(_rootUri), context.name().GetText()));
}
