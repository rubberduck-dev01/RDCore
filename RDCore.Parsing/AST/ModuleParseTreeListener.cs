using Antlr4.Runtime.Misc;
using RDCore.Parsing.Syntax;
using RDCore.SDK.Model;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.AST.Directives;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.Source;
using System.Collections.Immutable;
using System.Net.Mime;
using System.Xml.Linq;

namespace RDCore.Parsing.AST;

internal class NodeBuilder(Uri rootUri, string? nodeId = null)
{
    private readonly Uri _rootUri = rootUri;
    private readonly List<BoundNode> _children = [];

    public void AddChild(BoundNode node) => _children.Add(node);
    public void UpdateLastChild(BoundNode node)
    {
        if (node.GetType() != _children.Last().GetType())
        {
            // that would be an arbitrary replacement, most likely a bug.
            throw new InvalidOperationException();
        }
        _children.RemoveAt(_children.Count - 1);
        _children.Add(node);
    }

    public IEnumerable<BoundNode> GetChildren => _children.AsEnumerable();
    public BoundNode BuildImplementsDirective(VBAParser.ImplementsStmtContext context)
    {
        var name = context.expression().GetText().Split('.').Last(); // MS-VBAL: <class-type-name> (may be qualified)
        return new ImplementsDirectiveNode(
            GetUriWithFragmentFor($"implements-{name}"), 
            context.GetSourceLocation(_rootUri), 
            (BoundExpression)_children[0]);
    }
    public BoundNode BuildExternalDeclaration(VBAParser.DeclareStmtContext context)
    {
        var name = context.identifier().untypedIdentifier()?.GetText()
            ?? context.identifier().typedIdentifier().untypedIdentifier().GetText();
        var visibility = context.visibility()?.GetText();
        var kind = context.FUNCTION() is not null ? MemberKind.ExternalFunction : MemberKind.ExternalProcedure;
        var isPtrSafe = context.PTRSAFE() is not null;
        var literals = context.STRINGLITERAL();
        var lib = literals[0].GetText();
        var alias = literals.Length > 1 ? literals[1].GetText() : null;

        var location = context.GetSourceLocation(_rootUri);
        var modifier = string.IsNullOrWhiteSpace(visibility)
            ? AccessModifier.Implicit
            : Enum.Parse<AccessModifier>(visibility, ignoreCase: true);

        return new ExternalMemberDeclarationNode(
            GetUriWithFragmentFor(name), 
            location, 
            [.. _children],
            name, 
            lib, 
            isPtrSafe, 
            kind, 
            alias, 
            modifier);
    }
    public BoundNode BuildEventDeclaration(VBAParser.EventStmtContext context)
    {
        var name = context.identifier().untypedIdentifier()?.GetText()
                ?? context.identifier().typedIdentifier().untypedIdentifier().GetText();
        var modifier = context.visibility()?.GetText() is string value
            ? Enum.Parse<AccessModifier>(value) : AccessModifier.Implicit;

        return new MemberDeclarationNode(
            GetUriWithFragmentFor($"{Tokens.Event}_{name}"), 
            context.GetSourceLocation(_rootUri), 
            [.. _children], 
            name, 
            MemberKind.Event, 
            modifier);
    }
    public BoundNode BuildUserDefinedTypeDeclaration(VBAParser.UdtDeclarationContext context)
    {
        var name = context.untypedIdentifier().GetText();
        var modifier = context.visibility()?.GetText() is string value
            ? Enum.Parse<AccessModifier>(value) : AccessModifier.Implicit;

        return new MemberDeclarationNode(
            GetUriWithFragmentFor($"{Tokens.Type}_{name}"),
            context.GetSourceLocation(_rootUri),
            [.. _children],
            name, 
            MemberKind.UserDefinedType, 
            modifier);
    }
    public BoundNode BuildEnumDeclaration(VBAParser.EnumerationStmtContext context)
    {
        var name = context.identifier().untypedIdentifier()?.GetText()
            ?? context.identifier().typedIdentifier().untypedIdentifier().GetText();

        var modifier = context.visibility()?.GetText() is string value
            ? Enum.Parse<AccessModifier>(value) : AccessModifier.Implicit;
        
        return new MemberDeclarationNode(
            GetUriWithFragmentFor($"{Tokens.Enum}_{name}"),
            context.GetSourceLocation(_rootUri),
            [.. _children],
            name, 
            MemberKind.Enum, 
            modifier);
    }
    public BoundNode BuildParameterDeclaration(VBAParser.ArgContext context, bool isPropertyWriterMember = false, bool isLast = false)
    {
        var name = context.unrestrictedIdentifier().identifier().untypedIdentifier()?.GetText()
            ?? context.unrestrictedIdentifier().identifier().typedIdentifier().GetText();
        
        var kind = context.BYVAL() is not null ? ParameterKind.ExplicitByVal
            : context.BYREF() is not null ? ParameterKind.ExplicitByRef
                : isPropertyWriterMember && isLast
                    ? ParameterKind.ImplicitByVal
                    : ParameterKind.ImplicitByRef;
        
        return new ParameterDeclarationNode(
                    GetUriWithFragmentFor($"parameter_{name}"),
                    context.GetSourceLocation(_rootUri),
                    name,
                    kind,
                    context.OPTIONAL() is not null,
                    context.PARAMARRAY() is not null,
                    [.. _children]);
    }
    public BoundNode BuildPropertyGetDeclaration(VBAParser.PropertyGetStmtContext context)
    {
        var name = context.functionName().identifier().untypedIdentifier()?.GetText()
            ?? context.functionName().identifier().typedIdentifier().untypedIdentifier().GetText();

        var modifier = context.visibility()?.GetText() is string value
            ? Enum.Parse<AccessModifier>(value) : AccessModifier.Implicit;

        return new MemberDeclarationNode(
            GetUriWithFragmentFor($"get_{name}"),
            context.GetSourceLocation(_rootUri),
            [.. _children], 
            name,
            MemberKind.PropertyGet, 
            modifier);
    }
    public BoundNode BuildPropertyLetDeclaration(VBAParser.PropertyLetStmtContext context)
    {
        var name = context.subroutineName().identifier().untypedIdentifier()?.GetText()
            ?? context.subroutineName().identifier().typedIdentifier().untypedIdentifier().GetText();

        var modifier = context.visibility()?.GetText() is string value
            ? Enum.Parse<AccessModifier>(value) : AccessModifier.Implicit;

        return new MemberDeclarationNode(
            GetUriWithFragmentFor($"let_{name}"),
            context.GetSourceLocation(_rootUri),
            [.. _children],
            name,
            MemberKind.PropertyLet,
            modifier);
    }
    public BoundNode BuildPropertySetDeclaration(VBAParser.PropertySetStmtContext context)
    {
        var name = context.subroutineName().identifier().untypedIdentifier()?.GetText()
            ?? context.subroutineName().identifier().typedIdentifier().untypedIdentifier().GetText();

        var modifier = context.visibility()?.GetText() is string value
            ? Enum.Parse<AccessModifier>(value) : AccessModifier.Implicit;

        return new MemberDeclarationNode(
            GetUriWithFragmentFor($"set_{name}"),
            context.GetSourceLocation(_rootUri),
            [.. _children],
            name,
            MemberKind.PropertySet,
            modifier);
    }
    public BoundNode BuildProcedureDeclaration(VBAParser.SubStmtContext context)
    {
        var name = context.subroutineName().identifier().untypedIdentifier()?.GetText()
            ?? context.subroutineName().identifier().typedIdentifier().untypedIdentifier().GetText();

        var modifier = context.visibility()?.GetText() is string value
            ? Enum.Parse<AccessModifier>(value) : AccessModifier.Implicit;

        return new MemberDeclarationNode(
            GetUriWithFragmentFor($"{Tokens.Sub}_{name}"),
            context.GetSourceLocation(_rootUri),
            [.. _children],
            name,
            MemberKind.Procedure,
            modifier);
    }
    public BoundNode BuildFunctionDeclaration(VBAParser.FunctionStmtContext context)
    {
        var name = context.functionName().identifier().untypedIdentifier()?.GetText()
            ?? context.functionName().identifier().typedIdentifier().untypedIdentifier().GetText();

        var modifier = context.visibility()?.GetText() is string value
            ? Enum.Parse<AccessModifier>(value) : AccessModifier.Implicit;

        return new MemberDeclarationNode(
            GetUriWithFragmentFor($"{Tokens.Function}_{name}"),
            context.GetSourceLocation(_rootUri),
            [.. _children],
            name,
            MemberKind.Function,
            modifier);
    }
    private Uri GetUriWithFragmentFor(string name) => new($"{_rootUri.AbsolutePath.TrimEnd('#')}{(nodeId is not null ? $"/{nodeId}#" : "#")}{name.ToLowerInvariant()}");
}

internal class ModuleParseTreeListener(ModuleNode moduleNode) : VBAParserBaseListener
{
    private readonly ModuleNode _root = moduleNode;
    private readonly Stack<NodeBuilder> _builderStack = new([new(moduleNode.SemanticId)]);
    private NodeBuilder CurrentBuilder => _builderStack.Peek();
    private Uri GetUriWithFragmentFor(string name) => new($"{_root.SemanticId.AbsolutePath}#{name.ToLowerInvariant()}");

    private void OnEnterParent(string name)
    {
        _builderStack.Push(new(GetParentUriFor($"{name}_{_expressionId}")));
        _expressionId++;
    }
    private void OnExitParent(VBABaseParserRuleContext context, Func<NodeBuilder, BoundNode> provider) 
        => CurrentBuilder.AddChild(provider(_builderStack.Pop()));

    private Uri GetParentUriFor(string name) => new($"{_root.SemanticId.AbsolutePath}/{name.ToLowerInvariant()}");

    #region module directives
    private void OnModuleOptionDirective(string name, SourceLocation location, ModuleOptions value) 
        => CurrentBuilder.AddChild(new ModuleOptionDirectiveNode(GetUriWithFragmentFor(name), location, value));
    private void OnTypeDefDirective(string token, SourceLocation location, IEnumerable<(char from, char? to)> mappings, DefTypeUniversalPrefixMapping? universalMapping = default)
    {
        var prefixMappings = (universalMapping is null ? [] : new[] { universalMapping }).Concat(
            mappings.Select(map => new DefTypePrefixMapping(map.from, map.to))).ToImmutableArray();
        CurrentBuilder.AddChild(new TypeDefDirectiveNode(GetUriWithFragmentFor(token), location, token, prefixMappings));
    }

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
        => OnExitParent(context, builder => builder.BuildImplementsDirective(context));
    #endregion

    #region declarations
    public override void EnterDeclareStmt([NotNull] VBAParser.DeclareStmtContext context)
        => OnEnterParent(Tokens.Declare);
    public override void ExitDeclareStmt([NotNull] VBAParser.DeclareStmtContext context)
        => OnExitParent(context, builder => builder.BuildExternalDeclaration(context));

    public override void EnterEventStmt([NotNull] VBAParser.EventStmtContext context)
        => OnEnterParent(Tokens.Event);
    public override void ExitEventStmt([NotNull] VBAParser.EventStmtContext context)
        => OnExitParent(context, builder => builder.BuildEventDeclaration(context));

    public override void EnterUdtDeclaration([NotNull] VBAParser.UdtDeclarationContext context)
        => OnEnterParent(Tokens.Type);
    public override void ExitUdtDeclaration([NotNull] VBAParser.UdtDeclarationContext context)
        => OnExitParent(context, builder => builder.BuildUserDefinedTypeDeclaration(context));

    public override void EnterEnumerationStmt([NotNull] VBAParser.EnumerationStmtContext context)
        => OnEnterParent(Tokens.Enum);
    public override void ExitEnumerationStmt([NotNull] VBAParser.EnumerationStmtContext context)
        => OnExitParent(context, builder => builder.BuildEnumDeclaration(context));
    #endregion

    #region members
    private bool _isPropertyWriterMember = false;
    public override void EnterPropertyGetStmt([NotNull] VBAParser.PropertyGetStmtContext context)
        => OnEnterParent($"{Tokens.Property}{Tokens.Get}");
    public override void ExitPropertyGetStmt([NotNull] VBAParser.PropertyGetStmtContext context)
        => OnExitParent(context, builder => builder.BuildPropertyGetDeclaration(context));
    public override void EnterPropertyLetStmt([NotNull] VBAParser.PropertyLetStmtContext context)
    {
        OnEnterParent($"{Tokens.Property}{Tokens.Let}");
        _isPropertyWriterMember = true;
    }
    public override void ExitPropertyLetStmt([NotNull] VBAParser.PropertyLetStmtContext context)
    {
        _isPropertyWriterMember = false;
        OnExitParent(context, builder => builder.BuildPropertyLetDeclaration(context));
    }
    public override void EnterPropertySetStmt([NotNull] VBAParser.PropertySetStmtContext context)
    {
        OnEnterParent($"{Tokens.Property}{Tokens.Set}");
        _isPropertyWriterMember = true;
    }
    public override void ExitPropertySetStmt([NotNull] VBAParser.PropertySetStmtContext context)
    {
        _isPropertyWriterMember = false;
        OnExitParent(context, builder => builder.BuildPropertySetDeclaration(context));
    }

    public override void EnterSubStmt([NotNull] VBAParser.SubStmtContext context)
        => OnEnterParent(Tokens.Sub);
    public override void ExitSubStmt([NotNull] VBAParser.SubStmtContext context)
        => OnExitParent(context, builder => builder.BuildProcedureDeclaration(context));

    public override void EnterFunctionStmt([NotNull] VBAParser.FunctionStmtContext context)
        => OnEnterParent(Tokens.Function);
    public override void ExitFunctionStmt([NotNull] VBAParser.FunctionStmtContext context)
        => OnExitParent(context, builder => builder.BuildFunctionDeclaration(context));
    #endregion

    private int _expressionId = 0;
    private void OnBoundExpression(BoundExpression expression)
    {
        CurrentBuilder.AddChild(expression);
        _expressionId++;
    }

    public override void ExitSimpleNameExpr([NotNull] VBAParser.SimpleNameExprContext context)
    {
        var value = context.identifier().untypedIdentifier()?.GetText()
            ?? context.identifier().typedIdentifier().untypedIdentifier().GetText();
        var location = context.GetSourceLocation(_root.SemanticId);
        OnBoundExpression(new VBSimpleNameExpression(GetUriWithFragmentFor($"name_{value}_{_expressionId}"), location, value));
    }

    private int _parameterIndex = 0;
    public override void EnterArgList([NotNull] VBAParser.ArgListContext context)
    {
        _parameterIndex = 0;
    }
    public override void ExitArgList([NotNull] VBAParser.ArgListContext context)
    {
        _parameterIndex = 0;
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
        OnExitParent(context, builder => builder.BuildParameterDeclaration(context, isPropertyWriter));
        _parameterIndex++;
    }
}
