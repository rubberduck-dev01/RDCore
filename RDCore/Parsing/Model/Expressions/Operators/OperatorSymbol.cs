using RDCore.Parsing.Model.Types;
using RDCore.Server.ProtocolExtensions;
using static RDCore.Parsing.Model.Expressions.Operators.SymbolOperation;

namespace RDCore.Parsing.Model.Expressions.Operators;

internal abstract record class OperatorSymbol : StaticSymbol
{
    protected OperatorSymbol(string name, UnaryOperation unaryOp, VBType? vbType = default)
        : base(name, SymbolKindExt.Operator, vbType)
    {
        ExecuteUnaryOp = unaryOp;
    }

    protected OperatorSymbol(string name, BinaryOperation binaryOp, VBType? vbType = default)
        : base(name, SymbolKindExt.Operator, vbType)
    {
        ExecuteBinaryOp = binaryOp;
    }

    public virtual UnaryOperation ExecuteUnaryOp { get; init; } = default!;
    public virtual BinaryOperation ExecuteBinaryOp { get; init; } = default!;
}

internal abstract record class UnaryOperatorSymbol : OperatorSymbol
{
    protected UnaryOperatorSymbol(string name, UnaryOperation operation, VBType? vbType = null)
        : base(name, operation, vbType)
    {
    }
}

internal abstract record class BinaryOperatorSymbol : OperatorSymbol
{
    protected BinaryOperatorSymbol(string name, BinaryOperation operation, VBType? vbType = null)
        : base(name, operation, vbType)
    {
    }
}

internal abstract record class BitwiseOperatorSymbol : BinaryOperatorSymbol
{
    protected BitwiseOperatorSymbol(string name, BinaryOperation operation, VBType? vbType = null)
        : base(name, operation, vbType)
    {
    }
}

internal record class AdditionOperatorSymbol(VBType? VBType = default) : BinaryOperatorSymbol(Tokens.AdditionOp, SymbolOperation.EvaluateBinaryAddition) { }
internal record class SubtractionOperatorSymbol(VBType? VBType = default) : BinaryOperatorSymbol(Tokens.SubtractionOp, SymbolOperation.EvaluateBinarySubtraction) { }
internal record class MultiplicationOperatorSymbol(VBType? VBType = default) : BinaryOperatorSymbol(Tokens.MultiplicationOp, SymbolOperation.EvaluateBinaryMultiplication) { }
internal record class DivisionOperatorSymbol(VBType? VBType = default) : BinaryOperatorSymbol(Tokens.DivisionOp, SymbolOperation.EvaluateBinaryDivision) { }
internal record class IntegerDivisionOperatorSymbol(VBType? VBType = default) : BinaryOperatorSymbol(Tokens.IntegerDivisionOp, SymbolOperation.EvaluateBinaryIntegerDivision) { }
internal record class ExponentiationOperatorSymbol(VBType? VBType = default) : BinaryOperatorSymbol(Tokens.PowerOp, SymbolOperation.EvaluateBinaryExponentiation) { }
internal record class ModuloOperatorSymbol(VBType? VBType = default) : BinaryOperatorSymbol(Tokens.ModuloOp, SymbolOperation.EvaluateBinaryModulo) { }

internal record class ParenthesizedExpressionOperatorSymbol(VBType? VBType = default) : UnaryOperatorSymbol("(expression)", SymbolOperation.EvaluateUnaryParentheses) { }
internal record class UnaryPlusOperatorSymbol(VBType? VBType = default) : UnaryOperatorSymbol(Tokens.AdditionOp, SymbolOperation.EvaluateUnaryPlus) { }
internal record class UnaryMinusOperatorSymbol(VBType? VBType = default) : UnaryOperatorSymbol(Tokens.SubtractionOp, SymbolOperation.EvaluateUnaryMinus) { }
internal record class BitwiseNotOperatorSymbol(VBType? VBType = default) : UnaryOperatorSymbol(Tokens.LogicalNotOp, SymbolOperation.EvaluateUnaryBitwiseNot) { }
internal record class BitwiseEqvOperatorSymbol(VBType? VBType = default) : BinaryOperatorSymbol(Tokens.LogicalEqvOp, SymbolOperation.EvaluateBinaryBitwiseEqv) { }