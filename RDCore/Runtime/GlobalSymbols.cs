using RDCore.Parsing.Model;
using RDCore.Parsing.Model.Expressions.Operators;
using RDCore.Parsing.Model.Symbols;
using RDCore.Parsing.Model.Types;
using RDCore.Server.ProtocolExtensions;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace RDCore.Runtime;

internal static class GlobalSymbols
{
    // constants
    public static readonly StaticSymbol Empty = new(Tokens.Empty, SymbolKindExt.Constant, VBEmptyType.TypeInfo);
    public static readonly StaticSymbol Nothing = new(Tokens.Nothing, SymbolKindExt.Constant, VBObjectType.TypeInfo);
    public static readonly StaticSymbol Null = new(Tokens.Null, SymbolKindExt.Constant, VBNullType.TypeInfo);

    public static readonly ImmutableArray<OperatorSymbol> Operators =
    [
        new AdditionOperatorSymbol(),
        new SubtractionOperatorSymbol(),
        new MultiplicationOperatorSymbol(),
        new DivisionOperatorSymbol(),
        new IntegerDivisionOperatorSymbol(),
        new ExponentiationOperatorSymbol(),
        new ModuloOperatorSymbol(),
        new ParenthesizedExpressionOperatorSymbol(),
        new UnaryMinusOperatorSymbol(),
        new UnaryPlusOperatorSymbol(),
        new BitwiseNotOperatorSymbol(),
        new BitwiseAndOperatorSymbol(),
        new BitwiseOrOperatorSymbol(),
        new BitwiseXOrOperatorSymbol(),
        new BitwiseImpOperatorSymbol(),
        new BitwiseEqvOperatorSymbol(),
        new IsRefEqOperatorSymbol(),

        new MemberAccessOperatorSymbol(),
    ];

    public static void Initialize(ConcurrentDictionary<Uri, Symbol> index)
    {
        if (!index.IsEmpty)
        {
            throw new InvalidOperationException("The specified index dictionary is already initialized; index is expected to be empty.");
        }

        foreach (var op in Operators)
        {
            index[op.Uri] = op;
        }
    }
}