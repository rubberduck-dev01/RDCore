using RDCore.Parsing.Model.Symbols;

namespace RDCore.Parsing.Model.Values;

internal record class VBNothingValue : VBObjectValue,
    IVBTypedValue<VBObjectValue, VBLongPtrValue>
{
    public VBNothingValue(Symbol? symbol = null)
        : base(symbol, VBLongPtrValue.Zero)
    {
    }
}