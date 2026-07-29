namespace RDCore.Parsing.PreProcessing.Legacy;

internal sealed class CLngPtrLibraryFunctionExpression : Expression
{
    private readonly IExpression _expression;

    public CLngPtrLibraryFunctionExpression(IExpression expression)
    {
        _expression = expression;
    }

    public override IValue Evaluate()
    {
        return new CCurLibraryFunctionExpression(_expression).Evaluate();
    }
}
