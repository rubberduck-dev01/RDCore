using System.Globalization;

namespace RDCore.Parsing.PreProcessing.Legacy;

internal sealed class HexNumberLiteralExpression : Expression
{
    private readonly IExpression _tokenText;

    public HexNumberLiteralExpression(IExpression tokenText)
    {
        _tokenText = tokenText;
    }

    public override IValue Evaluate()
    {
        string literal = _tokenText.Evaluate().AsString;
        literal = literal.Replace("&H", string.Empty)
            .Replace("&", string.Empty)
            .Replace("%", string.Empty)
            .Replace("^", string.Empty);
        var number = int.Parse(literal, NumberStyles.HexNumber);
        return new DecimalValue(number);
    }
}
