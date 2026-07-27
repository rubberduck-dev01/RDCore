using Antlr4.Runtime.Misc;
using RDCore.Parsing.Syntax;
using RDCore.SDK.Model;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.AST.Directives;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.Source;
using RDCore.SDK.Model.Values.Intrinsic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace RDCore.Parsing.AST;

/// <summary>
/// A <em>listener</em> that builds the AST nodes representing all the directives and declarations in a module.
/// </summary>
/// <param name="moduleNode">The root AST module node.</param>
internal class DeclarationsParseTreeListener(ModuleNode moduleNode) : VBAParserBaseListener
{
    private readonly ModuleNode _root = moduleNode;
    private readonly Stack<NodeBuilder> _builderStack = new([new(moduleNode.SemanticId)]);
    private NodeBuilder CurrentBuilder => _builderStack.Peek();
    private Uri GetUriWithFragmentFor(string name) => new($"{_root.SemanticId.AbsolutePath}#{name.ToLowerInvariant()}");

    public BoundNode BuildModuleNode()
    {
        Debug.Assert(_builderStack.Count == 1);
        return _root with { Children = [.. CurrentBuilder.GetChildren] };
    }

    private void OnEnterParent(string name)
    {
        _builderStack.Push(new(GetParentUriFor($"{name}_{_expressionId}")));
        _expressionId++;
    }
    private void OnExitParent(Func<NodeBuilder, BoundNode> provider)
        => CurrentBuilder.AddChild(provider(_builderStack.Pop()));

    private Uri GetParentUriFor(string name) => new($"{_root.SemanticId.AbsolutePath}/{name.ToLowerInvariant()}");

    private bool _isInsideProcedure = false;
    private bool _isAfterArgsList = false;
    private bool IsDeclarationPassExpression => !_isInsideProcedure || _isAfterArgsList;

    private void OnModuleOptionDirective(string name, SourceLocation location, ModuleOptions value) 
        => CurrentBuilder.AddChild(new ModuleOptionDirectiveNode(GetUriWithFragmentFor(name), location, value));
    private void OnTypeDefDirective(string token, SourceLocation location, IEnumerable<(char from, char? to)> mappings, DefTypeUniversalPrefixMapping? universalMapping = default)
    {
        var prefixMappings = (universalMapping is null ? [] : new[] { universalMapping }).Concat(
            mappings.Select(map => new DefTypePrefixMapping(map.from, map.to))).ToImmutableArray();
        CurrentBuilder.AddChild(new TypeDefDirectiveNode(GetUriWithFragmentFor(token), location, token, prefixMappings));
    }

    public override void EnterAttributeStmt([NotNull] VBAParser.AttributeStmtContext context)
        => OnEnterParent(Tokens.Attribute);
    public override void ExitAttributeStmt([NotNull] VBAParser.AttributeStmtContext context)
        => OnExitParent(builder => builder.BuildAttributeDirective(context));
    public override void ExitOptionBaseStmt([NotNull] VBAParser.OptionBaseStmtContext context)
    {
        var location = context.GetSourceLocation(_root.SemanticId);
        var value = int.Parse(context.numberLiteral()?.INTEGERLITERAL()?.GetText() ?? "0");
        OnModuleOptionDirective($"{Tokens.Option}-{Tokens.Compare}", location, value == 1 ? ModuleOptions.OptionBase1 : ModuleOptions.OptionBase0);
    }
    public override void ExitOptionCompareStmt([NotNull] VBAParser.OptionCompareStmtContext context)
    {
        var location = context.GetSourceLocation(_root.SemanticId);
        var value = context.TEXT() is not null ? ModuleOptions.OptionCompareText
                : context.DATABASE() is not null ? ModuleOptions.OptionCompareDatabase
                : ModuleOptions.OptionCompareBinary;

        OnModuleOptionDirective($"{Tokens.Option}-{Tokens.Compare}", location, value);
    }
    public override void ExitOptionExplicitStmt([NotNull] VBAParser.OptionExplicitStmtContext context)
    {
        var location = context.GetSourceLocation(_root.SemanticId);
        OnModuleOptionDirective($"{Tokens.Option}-{Tokens.Explicit}", location, ModuleOptions.OptionExplicit);
    }
    public override void ExitOptionPrivateModuleStmt([NotNull] VBAParser.OptionPrivateModuleStmtContext context)
    {
        var location = context.GetSourceLocation(_root.SemanticId);
        OnModuleOptionDirective($"{Tokens.Option}-{Tokens.Private}", location, ModuleOptions.OptionPrivateModule);
    }
    public override void ExitDefDirective([NotNull] VBAParser.DefDirectiveContext context)
    {
        var token = context.defType().GetText();
        var location = context.GetSourceLocation(_root.SemanticId);

        DefTypeUniversalPrefixMapping? universalMapping = default;
        var mappings = new List<(char, char?)>([]);
        foreach (var spec in context.letterSpec())
        {
            if (spec.universalLetterRange() is not null)
            {
                universalMapping = new();
            }
            else if(spec.letterRange() is VBAParser.LetterRangeContext rangeContext)
            {
                var from = rangeContext.singleLetter()[0].GetText()[0];
                var to = rangeContext.singleLetter()[1].GetText()[0];
                mappings.Add(new(from, to));
            }
            else if (spec.singleLetter() is VBAParser.SingleLetterContext specContext)
            {
                mappings.Add(new(specContext.GetText()[0], null));
            }
        }
        OnTypeDefDirective(token, location, mappings, universalMapping);
    }

    public override void EnterImplementsStmt([NotNull] VBAParser.ImplementsStmtContext context) 
        => OnEnterParent(Tokens.Implements);
    public override void ExitImplementsStmt([NotNull] VBAParser.ImplementsStmtContext context)
        => OnExitParent(builder => builder.BuildImplementsDirective(context));


    public override void EnterDeclareStmt([NotNull] VBAParser.DeclareStmtContext context)
        => OnEnterParent(Tokens.Declare);
    public override void ExitDeclareStmt([NotNull] VBAParser.DeclareStmtContext context)
        => OnExitParent(builder => builder.BuildExternalDeclaration(context));

    public override void EnterEventStmt([NotNull] VBAParser.EventStmtContext context)
        => OnEnterParent(Tokens.Event);
    public override void ExitEventStmt([NotNull] VBAParser.EventStmtContext context)
        => OnExitParent(builder => builder.BuildEventDeclaration(context));

    public override void EnterUdtDeclaration([NotNull] VBAParser.UdtDeclarationContext context)
        => OnEnterParent(Tokens.Type);
    public override void ExitUdtDeclaration([NotNull] VBAParser.UdtDeclarationContext context)
        => OnExitParent(builder => builder.BuildUserDefinedTypeDeclaration(context));

    public override void EnterEnumerationStmt([NotNull] VBAParser.EnumerationStmtContext context)
        => OnEnterParent(Tokens.Enum);
    public override void ExitEnumerationStmt([NotNull] VBAParser.EnumerationStmtContext context)
        => OnExitParent(builder => builder.BuildEnumDeclaration(context));


    private bool _isPropertyWriterMember = false;
    private void OnEnterProcedure(string name, bool isPropertyWriter = false)
    {
        _isInsideProcedure = true;
        _isAfterArgsList = false;
        _isPropertyWriterMember = isPropertyWriter;
        OnEnterParent(name);
    }
    private void OnExitProcedure(Func<NodeBuilder, BoundNode> provider)
    {
        OnExitProcedure(provider);
        _isInsideProcedure = false;
        _isAfterArgsList = false;
        _isPropertyWriterMember = false;
    }

    public override void EnterPropertyGetStmt([NotNull] VBAParser.PropertyGetStmtContext context)
        => OnEnterProcedure($"{Tokens.Property}{Tokens.Get}");

    public override void ExitPropertyGetStmt([NotNull] VBAParser.PropertyGetStmtContext context) 
        => OnExitProcedure(builder => builder.BuildPropertyGetDeclaration(context));

    public override void EnterPropertyLetStmt([NotNull] VBAParser.PropertyLetStmtContext context) 
        => OnEnterProcedure($"{Tokens.Property}{Tokens.Let}", isPropertyWriter: true);
    public override void ExitPropertyLetStmt([NotNull] VBAParser.PropertyLetStmtContext context) 
        => OnExitProcedure(builder => builder.BuildPropertyLetDeclaration(context));
    public override void EnterPropertySetStmt([NotNull] VBAParser.PropertySetStmtContext context) 
        => OnEnterProcedure($"{Tokens.Property}{Tokens.Set}", isPropertyWriter: true);
    public override void ExitPropertySetStmt([NotNull] VBAParser.PropertySetStmtContext context) 
        => OnExitProcedure(builder => builder.BuildPropertySetDeclaration(context));

    public override void EnterSubStmt([NotNull] VBAParser.SubStmtContext context)
        => OnEnterProcedure(Tokens.Sub);
    public override void ExitSubStmt([NotNull] VBAParser.SubStmtContext context)
        => OnExitProcedure(builder => builder.BuildProcedureDeclaration(context));

    public override void EnterFunctionStmt([NotNull] VBAParser.FunctionStmtContext context)
        => OnEnterProcedure(Tokens.Function);
    public override void ExitFunctionStmt([NotNull] VBAParser.FunctionStmtContext context)
        => OnExitProcedure(builder => builder.BuildFunctionDeclaration(context));


    private int _expressionId = 0;
    private void OnBoundExpression(BoundExpression expression)
    {
        CurrentBuilder.AddChild(expression);
        _expressionId++;
    }

    public override void ExitSimpleNameExpr([NotNull] VBAParser.SimpleNameExprContext context)
    {
        if (!IsDeclarationPassExpression)
        {
            return;
        }
        var value = context.identifier().untypedIdentifier()?.GetText()
            ?? context.identifier().typedIdentifier().untypedIdentifier().GetText();
        var location = context.GetSourceLocation(_root.SemanticId);
        OnBoundExpression(new VBSimpleNameExpression(GetUriWithFragmentFor($"__{value}_{_expressionId}"), location, value));
    }
    public override void ExitLiteralIdentifier([NotNull] VBAParser.LiteralIdentifierContext context)
    {
        if (!IsDeclarationPassExpression)
        {
            return;
        }

        var location = context.GetSourceLocation(_root.SemanticId);
        if (context.booleanLiteralIdentifier() is VBAParser.BooleanLiteralIdentifierContext boolLiteralContext)
        {
            if (boolLiteralContext.FALSE() is not null)
            {
                OnBoundExpression(new VBLiteralExpression(GetUriWithFragmentFor($"__literal_bool_{_expressionId}"), location, VBBooleanValue.False));
            }
            else if (boolLiteralContext.TRUE() is not null)
            {
                OnBoundExpression(new VBLiteralExpression(GetUriWithFragmentFor($"__literal_bool_{_expressionId}"), location, VBBooleanValue.True));
            }
        }
        else if (context.objectLiteralIdentifier() is VBAParser.ObjectLiteralIdentifierContext objLiteralContext)
        {
            if (objLiteralContext.NOTHING() is not null)
            {
                OnBoundExpression(new VBLiteralExpression(GetUriWithFragmentFor($"__literal_object_{_expressionId}"), location, VBObjectValue.Nothing));
            }
        }
        else if (context.variantLiteralIdentifier() is VBAParser.VariantLiteralIdentifierContext variantLiteralContext)
        {
            if (variantLiteralContext.EMPTY() is not null)
            {
                OnBoundExpression(new VBLiteralExpression(GetUriWithFragmentFor($"__literal_variant_{_expressionId}"), location, VBEmptyValue.Empty));
            }
        }
    }
    

    private int _parameterIndex = 0;
    public override void EnterArgList([NotNull] VBAParser.ArgListContext context)
    {
        _parameterIndex = 0;
        _isAfterArgsList = false;
    }
    public override void ExitArgList([NotNull] VBAParser.ArgListContext context)
    {
        _parameterIndex = 0;
        _isAfterArgsList = true;
        if (_isPropertyWriterMember)
        {
            if (CurrentBuilder.GetChildren.Last() is ParameterDeclarationNode node
                && node.ParameterKind == ParameterKind.ImplicitByRef)
            {
                // we cannot do this before knowing how many parameters there are,
                // because it's only applicable to the RHS/value parameter (last).
                CurrentBuilder.UpdateLastChild(
                    node with { ParameterKind = ParameterKind.ImplicitByVal });
            }
        }
    }
    public override void EnterArg([NotNull] VBAParser.ArgContext context)
        => OnEnterParent($"parameter_{_parameterIndex}");
    public override void ExitArg([NotNull] VBAParser.ArgContext context)
    {
        var isPropertyWriter = _isPropertyWriterMember;
        OnExitParent(builder => builder.BuildParameterDeclaration(context, isPropertyWriter));
        _parameterIndex++;
    }
}
