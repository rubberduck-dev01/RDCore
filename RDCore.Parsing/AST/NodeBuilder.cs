using RDCore.Parsing.Syntax;
using RDCore.SDK.Model;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.AST.Directives;

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
    public BoundNode BuildAttributeDirective(VBAParser.AttributeStmtContext context)
    {
        // name may be qualified
        var identifiers = context.attributeName().GetText().Split('.');
        var name = identifiers.Length == 1 ? identifiers[0] : identifiers.Last();
        var qualifier = identifiers.Length == 2 ? identifiers[0] : null;
        return new AttributeDirectiveNode(
            GetUriWithFragmentFor($"{(qualifier is not null ? $"{qualifier}." : string.Empty)}{name}"),
            context.GetSourceLocation(_rootUri),
            name,
            _children[0],
            qualifier);
    }
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
