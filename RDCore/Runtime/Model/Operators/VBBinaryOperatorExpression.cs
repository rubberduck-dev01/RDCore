using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RDCore.Parsing.Model.Expressions.Operators;
using RDCore.Parsing.Model.Values;

namespace RDCore.Runtime.Model.Operators;

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

    public VBTypedValue Evaluate(VBExecutionContext context)
    {
        return SymbolOperation.EvaluateBinaryOpResult(
            context,
            Symbol,
            Left.ResultValue,
            Right.ResultValue
        );
    }
}
