using Antlr4.Runtime.Misc;
using RDCore.Parsing.Syntax;
using RDCore.SDK.Model;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Declarations;
using RDCore.SDK.Model.AST.Directives;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.Source;
using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Model.Symbols.Unbound;
using System.Collections.Immutable;

namespace RDCore.Parsing.AST;

public record class ParserEventInfo(BoundNode Node);

internal class ModuleParseTreeListener(VBModuleSymbol moduleSymbol) : VBAParserBaseListener
{
    private readonly VBModuleSymbol _root = moduleSymbol;
    private readonly Stack<Uri> _uriStack = new([moduleSymbol.Uri]);

    private readonly Stack<List<BoundNode>> _children = new([[]]);

    public ModuleNode CreateModuleTree() => new(_root.Uri, [.. _children.Pop()]);

    private Uri GetUriWithFragmentFor(string name) => new($"{_root.Uri.AbsolutePath}#{name.ToLowerInvariant()}");

    #region module directives
    private void OnModuleOptionDirective(string name, SourceLocation location, ModuleOptions value)
    {
        var node = new ModuleOptionDirectiveNode(GetUriWithFragmentFor(name), location, value);
        _children.Peek().Add(node);
    }
    private void OnTypeDefDirective(string token, SourceLocation location, IEnumerable<(char from, char? to)> mappings, DefTypeUniversalPrefixMapping? universalMapping = default)
    {
        var prefixMappings = (universalMapping is null ? [] : new[] { universalMapping }).Concat(
            mappings.Select(map => new DefTypePrefixMapping(map.from, map.to))).ToImmutableArray();
        var node = new TypeDefDirectiveNode(GetUriWithFragmentFor(token), location, token, prefixMappings);
        _children.Peek().Add(node);
    }
    private void OnImplementsDirective(string name, SourceLocation location, BoundExpression expression)
    {
        _children.Peek().Add(
            new ImplementsDirectiveNode(GetUriWithFragmentFor(name), location, expression));
    }

    private void OnDeclareStatement(string name, SourceLocation location, string? alias, string library, string? visibility, bool isPtrSafe, string token, BoundExpression? asType = null)
    {
        //_children.Peek().Add();
    }

    public override void ExitOptionBaseStmt([NotNull] VBAParser.OptionBaseStmtContext context)
    {
        var location = context.GetSourceLocation(_root.Uri);
        var value = int.Parse(context.numberLiteral()?.INTEGERLITERAL()?.GetText() ?? "0");
        OnModuleOptionDirective($"{Tokens.Option}-{Tokens.Compare}", location, value == 1 ? ModuleOptions.OptionBase1 : ModuleOptions.OptionBase0);
    }
    public override void ExitOptionCompareStmt([NotNull] VBAParser.OptionCompareStmtContext context)
    {
        var location = context.GetSourceLocation(_root.Uri);
        var value = context.TEXT() is not null ? ModuleOptions.OptionCompareText
                : context.DATABASE() is not null ? ModuleOptions.OptionCompareDatabase
                : ModuleOptions.OptionCompareBinary;

        OnModuleOptionDirective($"{Tokens.Option}-{Tokens.Compare}", location, value);
    }
    public override void ExitOptionExplicitStmt([NotNull] VBAParser.OptionExplicitStmtContext context)
    {
        var location = context.GetSourceLocation(_root.Uri);
        OnModuleOptionDirective($"{Tokens.Option}-{Tokens.Explicit}", location, ModuleOptions.OptionExplicit);
    }
    public override void ExitOptionPrivateModuleStmt([NotNull] VBAParser.OptionPrivateModuleStmtContext context)
    {
        var location = context.GetSourceLocation(_root.Uri);
        OnModuleOptionDirective($"{Tokens.Option}-{Tokens.Private}", location, ModuleOptions.OptionPrivateModule);
    }

    public override void ExitDefDirective([NotNull] VBAParser.DefDirectiveContext context)
    {
        var token = context.defType().GetText();
        var location = context.GetSourceLocation(_root.Uri);

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

    public override void ExitImplementsStmt([NotNull] VBAParser.ImplementsStmtContext context)
    {
        var name = context.expression().GetText().Split('.').Last(); // MS-VBAL: <class-type-name> (may be qualified)
        var expression = _expressionStack.Pop();

        OnImplementsDirective(name, context.GetSourceLocation(_root.Uri), expression);
    }
    #endregion

    #region declarations
    public override void ExitDeclareStmt([NotNull] VBAParser.DeclareStmtContext context)
    {
        var name = context.identifier().GetText();
        var visibility = context.visibility().GetText();
        var token = context.FUNCTION() is not null ? Tokens.Function : Tokens.Sub;
        var isPtrSafe = context.PTRSAFE() is not null;
        var literals = context.STRINGLITERAL();
        var lib = literals[0].GetText();
        var alias = literals.Length > 1 ? literals[1].GetText() : null;

        var location = context.GetSourceLocation(_root.Uri);
        OnDeclareStatement(name, location, alias, lib, visibility, isPtrSafe, token);
    }
    public override void ExitEventStmt([NotNull] VBAParser.EventStmtContext context)
    {
        var name = context.identifier().GetText();
        base.ExitEventStmt(context);
    }
    public override void ExitUdtDeclaration([NotNull] VBAParser.UdtDeclarationContext context)
    {
        base.ExitUdtDeclaration(context);
    }
    public override void ExitEnumerationStmt([NotNull] VBAParser.EnumerationStmtContext context)
    {
        base.ExitEnumerationStmt(context);
    }
    #endregion

    #region members
    #endregion

    private readonly Stack<BoundExpression> _expressionStack = [];
    private int _expressionId = 0;

    public override void ExitSimpleNameExpr([NotNull] VBAParser.SimpleNameExprContext context)
    {
        var name = context.identifier().GetText();
        var location = context.GetSourceLocation(_root.Uri);
        _expressionStack.Push(new VBSimpleNameExpression(GetUriWithFragmentFor($"__{name}_{_expressionId}"), location, name));
        _expressionId++;
    }
}
