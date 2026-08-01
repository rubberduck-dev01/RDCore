using Antlr4.Runtime;

namespace RDCore.Parsing.PreProcessing.Legacy;

internal sealed class EmptyValue : IValue
{
    public static readonly EmptyValue Value = new();

    public ValueType ValueType
    {
        get
        {
            return ValueType.Empty;
        }
    }

    public bool AsBool
    {
        get
        {
            return false;
        }
    }

    public byte AsByte
    {
        get
        {
            return 0;
        }
    }

    public DateTime AsDate
    {
        get
        {
            return new DateTime(1899, 12, 30);
        }
    }

    public decimal AsDecimal
    {
        get
        {
            return 0;
        }
    }

    public string AsString
    {
        get
        {
            return string.Empty;
        }
    }

    public IEnumerable<IToken> AsTokens
    {
        get
        {
            return new List<IToken>();
        }
    }

    public override string ToString()
    {
        return "<Empty>";
    }
}
