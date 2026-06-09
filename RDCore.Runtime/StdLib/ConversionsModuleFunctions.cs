using Newtonsoft.Json.Linq;
using RDCore.SDK.Model;
using RDCore.SDK.Model.AST.Abstract;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Model.Types;
using RDCore.SDK.Model.Types.Abstract;
using RDCore.SDK.Model.Values;
using RDCore.SDK.Model.Values.Abstract;
using RDCore.SDK.Model.Values.Intrinsic;
using RDCore.SDK.Model.Values.Meta;
using RDCore.SDK.Runtime;
using RDCore.SDK.Runtime.Abstract;
using RDCore.SDK.Semantics;
using RDCore.SDK.Semantics.Runtime.Abstract;
using RDCore.SDK.Semantics.Runtime.LetCoercion;
using RDCore.SDK.Semantics.Runtime.Operators.Context;
using RDCore.SDK.Server.ProtocolExtensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace RDCore.Runtime.StdLib;



/// <summary>
/// <strong>MS-VBAL 6.1.2.3 Conversion Module</strong><br/>
/// A service that provides <em>global scope</em> <see cref="StaticSymbol"/> references for the <c>Conversion</c> <em>standard library module</em>.
/// </summary>
public class ConversionModuleSymbols : IStaticSymbolsProvider
{
    private readonly Dictionary<string, StaticSymbol> _map;

    /// <summary>
    /// <strong>MS-VBAL 6.1.2.3.1 Public Functions</strong><br/>
    /// Defines the <em>explicit-coercion</em> functions.
    /// </summary>
    public static class PublicFunctions
    {
        private static readonly Lazy<StaticSymbol> _cbool = new(() => new(Tokens.CBool, SymbolKindExt.Function, VBBooleanType.TypeInfo), LazyThreadSafetyMode.PublicationOnly);
        /// <summary>
        /// <strong>MS-VBAL 6.1.2.3.1.1 CBool</strong><br/>
        /// A <see cref="StaticSymbol"/> representing the <see cref="VBBooleanType"/> <em>explicit type coercion</em> function.
        /// </summary>
        public static StaticSymbol CBool => _cbool.Value;
    }

    public ConversionModuleSymbols()
    {
        _map = new()
        {
            [PublicFunctions.CBool.Name] = PublicFunctions.CBool,
        };
    }

    public IEnumerable<StaticSymbol> GetAll() => [.. _map.Values];
    public bool TryGetByName(string name, [NotNullWhen(true)][MaybeNullWhen(false)] out StaticSymbol symbol) => _map.TryGetValue(name, out symbol);
}

public record class ConversionFunctionSemantics(
    IStaticSymbolsProvider symbolProvider, ISymbolResolver symbolResolver,
    ILetCoercionRuntimeSemanticsProvider letCoercionProvider)
    : RuntimeSemantics<ConversionOperationSemanticContext, ConversionSemanticFlags>()
{
    private readonly ISymbolResolver _resolver = symbolResolver;
    private readonly ILetCoercionRuntimeSemanticsProvider _letCoercion = letCoercionProvider;

    public override ISemanticFlagsAccumulator<ConversionSemanticFlags> Analyze(
        ISymbolResolver resolver, 
        ConversionOperationSemanticContext conversionContext, 
        ISemanticFlagsAccumulator<ConversionSemanticFlags> builder, 
        BoundNode<ConversionOperationSemanticContext, ConversionSemanticFlags> node, 
        params VBTypedValue[] inputs)
    {
        throw new NotImplementedException();
    }

    public override RuntimeSemanticsEvaluationResult Evaluate(
        IVBExecutionContext runtime, 
        SemanticContext<ConversionSemanticFlags> context, 
        BoundStatementNode<ConversionOperationSemanticContext, ConversionSemanticFlags> node, params VBTypedValue[] inputs)
    {
        var coercionResult = _letCoercion.EvaluateLetCoercionSemantics(_resolver, node, new LetCoercionStackFrame(
                NodeUri: node.SemanticId,
                OperatorSymbol: symbolProvider.TryGetByName(Tokens.CBool, out var symbol) ? symbol : throw new InvalidOperationException(),
                InputIndex: InputIndex.CTypeSourceValue,
                SourceValue: inputs[(int)InputIndex.CTypeSourceValue],
                DestinationTypeDesc: (VBTypeDescValue)inputs[(int)InputIndex.CTypeTargetType]));
        return coercionResult.IsSuccess
            ? RuntimeSemanticsEvaluationResult.Success(coercionResult.Result!)
            : RuntimeSemanticsEvaluationResult.Error(coercionResult.ErrorInfo ?? )
    }
}
