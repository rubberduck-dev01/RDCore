using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Model.Types;
using RDCore.SDK.Model.Values.Abstract;

namespace RDCore.SDK.Model.Values.Intrinsic;

/// <summary>
/// A <see cref="VBNumericTypedValue"/> representing a runtime value of the <see cref="VBCurrencyType"/> data type.
/// </summary>
/// <param name="Symbol">The <see cref="Symbol"/> associated with this value.</param>
public sealed record class VBCurrencyValue(Symbol Symbol) 
    : VBNumericTypedValue(VBCurrencyType.TypeInfo, Symbol), IVBTypedValue<VBCurrencyValue, decimal>, INumericValue<VBCurrencyValue>
{
    public decimal Value => ManagedValue.Decimal;
    public override int Size => sizeof(Decimal);

    public bool Equals(IVBTypedValue<VBCurrencyValue, decimal>? other) => Value == other?.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
