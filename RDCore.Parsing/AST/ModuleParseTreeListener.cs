using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RDCore.Parsing.Syntax;
using RDCore.SDK.Extensibility;
using RDCore.SDK.Model;
using RDCore.SDK.Model.AST;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Directives;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.Source;
using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Model.Symbols.Unbound;
using RDCore.SDK.Model.Symbols.VBProject;
using RDCore.SDK.Model.Types;
using RDCore.SDK.Semantics.Context;
using RDCore.SDK.Semantics.Flags;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace RDCore.Parsing.AST;

internal class ModuleParseTreeListener(VBModuleSymbol moduleSymbol) : VBAParserBaseListener
{
    private readonly VBModuleSymbol _root = moduleSymbol;
    private readonly Stack<Uri> _uriStack = new([moduleSymbol.Uri]);

    private readonly Stack<List<BoundNode>> _children = new([[]]);
    private readonly List<Symbol> _symbols = [];

    public ModuleNode CreateModuleTree() => new(_root.Uri, [.. _children.Pop()]);
    public ImmutableArray<Symbol> GetSymbols() => [.. _symbols];

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
        base.ExitDeclareStmt(context);
    }
    public override void ExitEventStmt([NotNull] VBAParser.EventStmtContext context)
    {
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
