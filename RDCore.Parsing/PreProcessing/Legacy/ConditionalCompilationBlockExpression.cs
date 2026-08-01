using Antlr4.Runtime;

namespace RDCore.Parsing.PreProcessing.Legacy;

internal sealed class ConditionalCompilationBlockExpression : Expression
{
    private readonly IEnumerable<IExpression> _children;

    public ConditionalCompilationBlockExpression(IEnumerable<IExpression> children)
    {
        _children = children;
    }

    public override IValue Evaluate()
    {
        var tokens = new List<IToken>();
        foreach(var child in _children)
        {
            tokens.AddRange(child.Evaluate().AsTokens);
        }
        return new TokensValue(tokens);
    }
}
