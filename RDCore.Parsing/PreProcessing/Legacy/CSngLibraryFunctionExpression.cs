namespace RDCore.Parsing.PreProcessing.Legacy;

internal sealed class CSngLibraryFunctionExpression : Expression
{
    private readonly IExpression _expression;

    public CSngLibraryFunctionExpression(IExpression expression)
    {
        _expression = expression;
    }

    public override IValue Evaluate()
    {
        return new CCurLibraryFunctionExpression(_expression).Evaluate();
    }
}
