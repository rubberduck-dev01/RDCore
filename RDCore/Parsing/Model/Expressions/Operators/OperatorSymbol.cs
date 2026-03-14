using RDCore.Server.ProtocolExtensions;
using static RDCore.Parsing.Model.Expressions.Operators.SymbolOperation;

namespace RDCore.Parsing.Model.Expressions.Operators;

internal abstract record class OperatorSymbol : StaticSymbol
{
    protected OperatorSymbol(string token, UnaryOperation unaryOp)
        : base(token, SymbolKindExt.Operator)
    {
        ExecuteUnaryOp = unaryOp;
    }

    protected OperatorSymbol(string token, BinaryOperation binaryOp)
        : base(token, SymbolKindExt.Operator)
    {
        ExecuteBinaryOp = binaryOp;
    }

    public virtual UnaryOperation ExecuteUnaryOp { get; init; } = default!;
    public virtual BinaryOperation ExecuteBinaryOp { get; init; } = default!;
}

internal abstract record class UnaryOperatorSymbol : OperatorSymbol
{
    protected UnaryOperatorSymbol(string token, UnaryOperation operation)
        : base(token, operation)
    {
    }
}

internal abstract record class BinaryOperatorSymbol : OperatorSymbol
{
    protected BinaryOperatorSymbol(string name, BinaryOperation operation)
        : base(name, operation)
    {
    }
}

internal abstract record class BitwiseOperatorSymbol : BinaryOperatorSymbol
{
    protected BitwiseOperatorSymbol(string name, BinaryOperation operation)
        : base(name, operation)
    {
    }
}

internal record class AdditionOperatorSymbol() : BinaryOperatorSymbol(Tokens.AdditionOp, SymbolOperation.EvaluateBinaryAddition) { }
internal record class SubtractionOperatorSymbol() : BinaryOperatorSymbol(Tokens.SubtractionOp, SymbolOperation.EvaluateBinarySubtraction) { }
internal record class MultiplicationOperatorSymbol() : BinaryOperatorSymbol(Tokens.MultiplicationOp, SymbolOperation.EvaluateBinaryMultiplication) { }
internal record class DivisionOperatorSymbol() : BinaryOperatorSymbol(Tokens.DivisionOp, SymbolOperation.EvaluateBinaryDivision) { }
internal record class IntegerDivisionOperatorSymbol() : BinaryOperatorSymbol(Tokens.IntegerDivisionOp, SymbolOperation.EvaluateBinaryIntegerDivision) { }
internal record class ExponentiationOperatorSymbol() : BinaryOperatorSymbol(Tokens.PowerOp, SymbolOperation.EvaluateBinaryExponentiation) { }
internal record class ModuloOperatorSymbol() : BinaryOperatorSymbol(Tokens.ModuloOp, SymbolOperation.EvaluateBinaryModulo) { }

internal record class ParenthesizedExpressionOperatorSymbol() : UnaryOperatorSymbol("(expression)", SymbolOperation.EvaluateUnaryParentheses) { }
internal record class UnaryPlusOperatorSymbol() : UnaryOperatorSymbol(Tokens.AdditionOp, SymbolOperation.EvaluateUnaryPlus) { }
internal record class UnaryMinusOperatorSymbol() : UnaryOperatorSymbol(Tokens.SubtractionOp, SymbolOperation.EvaluateUnaryMinus) { }
internal record class BitwiseNotOperatorSymbol() : UnaryOperatorSymbol(Tokens.LogicalNotOp, SymbolOperation.EvaluateUnaryBitwiseNot) { }
internal record class BitwiseAndOperatorSymbol() : BinaryOperatorSymbol(Tokens.LogicalAndOp, SymbolOperation.EvaluateBinaryBitwiseAnd) { }
internal record class BitwiseOrOperatorSymbol() : BinaryOperatorSymbol(Tokens.LogicalOrOp, SymbolOperation.EvaluateBinaryBitwiseOr) { }
internal record class BitwiseXOrOperatorSymbol() : BinaryOperatorSymbol(Tokens.LogicalXOrOp, SymbolOperation.EvaluateBinaryBitwiseXOr) { }
internal record class BitwiseImpOperatorSymbol() : BinaryOperatorSymbol(Tokens.LogicalImpOp, SymbolOperation.EvaluateBinaryBitwiseImp) { }
internal record class BitwiseEqvOperatorSymbol() : BinaryOperatorSymbol(Tokens.LogicalEqvOp, SymbolOperation.EvaluateBinaryBitwiseEqv) { }

internal record class IsRefEqOperatorSymbol() : BinaryOperatorSymbol(Tokens.CompareIsOp, SymbolOperation.EvaluateBinaryIsRefEquality) { }
internal record class MemberAccessOperatorSymbol() : BinaryOperatorSymbol(Tokens.MemberAccess, SymbolOperation.EvaluateBinaryMemberAccess) { }