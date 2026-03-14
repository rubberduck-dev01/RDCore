using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RDCore.Parsing.Model.Expressions.Operators;
using RDCore.Parsing.Model.Values;

namespace RDCore.Runtime.Model.Operators;

internal abstract record class VBUnaryOperatorExpression : VBOperatorExpression
{
    protected VBUnaryOperatorExpression(OperatorSymbol symbol, Location location, ValuedExpression expression)
        : base(symbol, location)
    {
        Expression = expression;
    }

    public ValuedExpression Expression { get; init; }

    public override VBTypedValue Evaluate(VBExecutionContext context)
    {
        var value = Expression.Evaluate(context);
        return Symbol.ExecuteUnaryOp(context, this, value);
    }
}

internal record class VBBinaryOperatorExpression : VBOperatorExpression
{
    public VBBinaryOperatorExpression(OperatorSymbol symbol, ValuedExpression left, ValuedExpression right, Location location)
        : base(symbol, location)
    {
        Left = left;
        Right = right;
    }

    public ValuedExpression Left { get; init; }
    public ValuedExpression Right { get; init; }

    public override VBTypedValue Evaluate(VBExecutionContext context)
    {
        var lhsValue = Left.Evaluate(context);
        var rhsValue = Right.Evaluate(context);
        return Symbol.ExecuteBinaryOp(context, this, lhsValue, rhsValue);
    }
}

internal abstract record VBBitwiseOperatorExpression(OperatorSymbol symbol, ValuedExpression left, ValuedExpression right, Location location)
    : VBBinaryOperatorExpression(symbol, left, right, location)
{ }
