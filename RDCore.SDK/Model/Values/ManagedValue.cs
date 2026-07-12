using RDCore.SDK.Model.Values.Intrinsic;
using System.Runtime.InteropServices;
namespace RDCore.SDK.Model.Values;

[StructLayout(LayoutKind.Explicit)]
public readonly struct ManagedCurrency
{
    public ManagedCurrency(long storedValue)
    {
        StoredValue = storedValue;
    }

    public ManagedCurrency(decimal scaledValue)
    {
        StoredValue = Convert.ToInt64(scaledValue * 10000);
    }

    [FieldOffset(0)] public readonly long StoredValue;
    public decimal Value => StoredValue / 10000;
}

public readonly record struct ManagedWrapper
{
    public ManagedWrapper(ManagedValue value)
    {
        Value = value;
    }
    public ManagedWrapper(ManagedReference reference)
    {
        Reference = reference;
    }
    public ManagedWrapper(VBVariantInteropValue variant)
    {
        Variant = variant;
    }

    public ManagedValue? Value { get; init; }
    public ManagedReference? Reference { get; init; }
    public VBVariantInteropValue? Variant { get; init; }
}

/// <summary>
/// The managed (.net) representation of a runtime reference.
/// </summary>
/// <remarks>
/// This <em>union</em> defines all its fields at offset 0.
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct ManagedReference
{
    public ManagedReference(VBReferenceInteropValue reference)
    {
        Ref = reference;
    }

    [FieldOffset(0)] public readonly VBReferenceInteropValue Ref;

    public static ManagedReference NullRefValue { get; } = new(new(typeof(object), Symbols.Abstract.ScopeKind.Unallocated, null!));
    public static ManagedReference EmptyStringRefValue { get; } = new(new(typeof(string), Symbols.Abstract.ScopeKind.Unallocated, string.Empty));
}

/// <summary>
/// The managed (.net) representation of a runtime value.
/// </summary>
/// <remarks>
/// This <em>union</em> defines all its fields at offset 0.
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct ManagedValue
{
    public ManagedValue(bool booleanValue)
    {
        Boolean = booleanValue;
    }
    public ManagedValue(byte byteValue)
    {
        Byte = byteValue;
    }
    public ManagedValue(short integerValue)
    {
        Int16 = integerValue;
    }
    public ManagedValue(int longValue)
    {
        Int32 = longValue;
    }
    public ManagedValue(long longLongValue)
    {
        Int64 = longLongValue;
    }
    public ManagedValue(float singleValue)
    {
        Single = singleValue;
    }
    public ManagedValue(double doubleValue)
    {
        Double = doubleValue;
    }
    public ManagedValue(ManagedCurrency currencyValue)
    {
        Currency = currencyValue;
    }

    [FieldOffset(0)] public readonly System.Boolean Boolean;
    [FieldOffset(0)] public readonly System.Byte Byte;
    [FieldOffset(0)] public readonly System.Int16 Int16;
    [FieldOffset(0)] public readonly System.Int32 Int32;
    [FieldOffset(0)] public readonly System.Int64 Int64;
    [FieldOffset(0)] public readonly System.Single Single;
    [FieldOffset(0)] public readonly System.Double Double;
    [FieldOffset(0)] public readonly ManagedCurrency Currency;

    public static ManagedValue BooleanFalse { get; } = new(false);
    public static ManagedValue BooleanTrue { get; } = new(true);
    public static ManagedValue ByteMinValue { get; } = new(byte.MinValue);
    public static ManagedValue ByteMaxValue { get; } = new(byte.MaxValue);
    public static ManagedValue ByteZeroValue { get; } = new((byte)0);
    public static ManagedValue Int16MinValue { get; } = new(short.MinValue);
    public static ManagedValue Int16MaxValue { get; } = new(short.MaxValue);
    public static ManagedValue Int16ZeroValue { get; } = new((short)0);
    public static ManagedValue Int32MinValue { get; } = new(int.MinValue);
    public static ManagedValue Int32MaxValue { get; } = new(int.MaxValue);
    public static ManagedValue Int32ZeroValue { get; } = new(0);
    public static ManagedValue Int64MinValue { get; } = new(long.MinValue);
    public static ManagedValue Int64MaxValue { get; } = new(long.MaxValue);
    public static ManagedValue Int64ZeroValue { get; } = new((long)0);
    public static ManagedValue SingleMinValue { get; } = new(float.MinValue);
    public static ManagedValue SingleMaxValue { get; } = new(float.MaxValue);
    public static ManagedValue SingleZeroValue { get; } = new(0f);
    public static ManagedValue DoubleMinValue { get; } = new(double.MinValue);
    public static ManagedValue DoubleMaxValue { get; } = new(double.MaxValue);
    public static ManagedValue DoubleZeroValue { get; } = new(0d);
    public static ManagedValue CurrencyMinValue { get; } = new(new ManagedCurrency(long.MinValue));
    public static ManagedValue CurrencyMaxValue { get; } = new(new ManagedCurrency(long.MaxValue));
    public static ManagedValue CurrencyZeroValue { get; } = new(new ManagedCurrency((long)0));
}
