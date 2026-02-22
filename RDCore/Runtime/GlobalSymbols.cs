using RDCore.Parsing.Model;
using RDCore.Parsing.Model.Expressions.Operators;
using RDCore.Parsing.Model.Symbols;
using RDCore.Parsing.Model.Types;
using RDCore.Server.ProtocolExtensions;
using System.Collections.Concurrent;

namespace RDCore.Runtime;

internal static class GlobalSymbols
{
    // operators
    public static readonly AdditionOperatorSymbol Addition = new();
    public static readonly SubtractionOperatorSymbol Subtraction = new();
    public static readonly MultiplicationOperatorSymbol Multiplication = new();
    public static readonly DivisionOperatorSymbol Division = new();
    public static readonly IntegerDivisionOperatorSymbol IntegerDivision = new();
    public static readonly ExponentiationOperatorSymbol Exponentiation = new();
    public static readonly ModuloOperatorSymbol Modulo = new();

    public static readonly ParenthesizedExpressionOperatorSymbol ParenthesizedExp = new();
    public static readonly UnaryPlusOperatorSymbol UnaryPlus = new();
    public static readonly UnaryPlusOperatorSymbol UnaryMinus = new();

    public static readonly BitwiseNotOperatorSymbol Not = new();
    public static readonly BitwiseEqvOperatorSymbol Eqv = new();


    // constants
    public static readonly StaticSymbol Empty = new(Tokens.Empty, SymbolKindExt.Constant, VBEmptyType.TypeInfo);
    public static readonly StaticSymbol Nothing = new(Tokens.Nothing, SymbolKindExt.Constant, VBObjectType.TypeInfo);
    public static readonly StaticSymbol Null = new(Tokens.Null, SymbolKindExt.Constant, VBNullType.TypeInfo);

    public static void Initialize(ConcurrentDictionary<Uri, Symbol> index)
    {
        if (!index.IsEmpty)
        {
            throw new InvalidOperationException("The specified index dictionary is already initialized; index is expected to be empty.");
        }

        index[Addition.Uri] = Addition;
        index[Subtraction.Uri] = Subtraction;
        index[Multiplication.Uri] = Multiplication;
        index[Division.Uri] = Division;
        index[IntegerDivision.Uri] = IntegerDivision;
        index[Exponentiation.Uri] = Exponentiation;
        index[Modulo.Uri] = Modulo;

        index[ParenthesizedExp.Uri] = ParenthesizedExp;
        index[UnaryPlus.Uri] = UnaryPlus;
        index[UnaryMinus.Uri] = UnaryMinus;

        index[Null.Uri] = Null;
        index[Empty.Uri] = Empty;
    }
}