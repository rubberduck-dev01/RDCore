using RDCore.Parsing.Model.Symbols;
using RDCore.Parsing.Model.Types;
using RDCore.Parsing.Model.Types.Complex;

namespace RDCore.Parsing.Model.Values;

internal record class VBObjectValue : VBTypedValue,
    IVBTypedValue<VBObjectValue, VBLongPtrValue>,
    INumericCoercion,
    IStringCoercion
{
    public VBObjectValue(Symbol? symbol, VBLongPtrValue? address = default)
        : base(VBObjectType.TypeInfo, symbol)
    {
        Value = address ?? Nothing.Value;
    }

    public static VBObjectValue Nothing { get; } = new VBNothingValue();

    public VBLongPtrValue Value { get; init; }
    public override int Size => VBLongPtrValue.BitnessAwarePtrSize;

    public bool IsNothing() => Value == Nothing.Value;

    public VBDoubleValue? AsCoercedNumeric(int depth = 0) => LetCoerce(depth) is INumericValue value ? value.AsDouble() : null;
    public VBStringValue? AsCoercedString(int depth = 0) => LetCoerce(depth) is VBStringValue value ? value : null;

    /// <summary>
    /// Implicit default member call coerces the object reference into an intrinsic value.
    /// </summary>
    /// <remarks>
    /// Let coercion is recursive: a class type's default member may be another class type with a default member.
    /// </remarks>
    public VBTypedValue LetCoerce(int depth = 0)
    {
        if (depth >= 9) // TODO configure
        {
            throw VBRuntimeErrorException.OutOfStackSpace(Symbol?.SelectionRange!, $"Recursive `Let` coercion did not resolve a typed value, {depth} levels deep.");
        }

        if (IsNothing())
        {
            throw VBRuntimeErrorException.ObjectVariableNotSet(Symbol?.SelectionRange!, $"Recursive `Let` coercion requires the object reference to be assigned so that the default member can be invoked.");
        }

        if (TypeInfo is VBClassType classType && classType.DefaultMember != null)
        {
            var symbol = classType.DefaultMember as Symbol;
            if (classType.DefaultMember is VBReturningMember member)
            {
                if (member.ResolvedType is INumericCoercion coercibleNumeric)
                {
                    var value = coercibleNumeric.AsCoercedNumeric(depth);
                    if (symbol != null && value != null)
                    {
                        return new VBDoubleValue(symbol).WithValue(value.Value);
                    }
                }
                else if (member.ResolvedType is IStringCoercion coercibleString)
                {
                    var value = coercibleString.AsCoercedString(depth);
                    if (symbol != null && value != null)
                    {
                        return value;
                    }
                }
            }
        }
        throw VBRuntimeErrorException.ObjectDoesntSupportPropertyOrMethod(Symbol?.SelectionRange!, $"`Let` coercion requires an object type that defines a default member, but none was found.");
    }

    public bool Equals(IVBTypedValue<VBObjectValue, VBLongPtrValue>? other) => Value.Equals(other?.Value);
    public override int GetHashCode() => Value.GetHashCode();
}
