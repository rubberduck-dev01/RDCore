using RDCore.SDK.Model.Values.Intrinsic;
using System.Runtime.InteropServices;
namespace RDCore.SDK.Model.Values;

/// <summary>
/// The managed (.net) representation of a runtime value.
/// </summary>
/// <remarks>
/// This <em>union</em> defines all its fields at offset 0.
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public struct ManagedValue
{
    [FieldOffset(0)] public System.Boolean Boolean;
    [FieldOffset(0)] public System.Byte Byte;
    [FieldOffset(0)] public System.Int16 Int16;
    [FieldOffset(0)] public System.Int32 Int32;
    [FieldOffset(0)] public System.Int64 Int64;
    [FieldOffset(0)] public System.Single Single;
    [FieldOffset(0)] public System.Double Double;
    [FieldOffset(0)] public System.Decimal Decimal;
    [FieldOffset(0)] public VBVariantInteropValue Variant;
    [FieldOffset(0)] public VBReferenceInteropValue Ref;

    public static ManagedValue BooleanFalse { get; } = new() { Boolean = false };
    public static ManagedValue BooleanTrue { get; } = new() { Boolean = true };
    public static ManagedValue ByteMinValue { get; } = new() { Byte = byte.MinValue };
    public static ManagedValue ByteMaxValue { get; } = new() { Byte = byte.MaxValue };
    public static ManagedValue ByteZeroValue { get; } = new() { Byte = 0 };
    public static ManagedValue Int16MinValue { get; } = new() { Int16 = short.MinValue };
    public static ManagedValue Int16MaxValue { get; } = new() { Int16 = short.MaxValue };
    public static ManagedValue Int16ZeroValue { get; } = new() { Int16 = 0 };
    public static ManagedValue Int32MinValue { get; } = new() { Int32 = int.MinValue };
    public static ManagedValue Int32MaxValue { get; } = new() { Int32 = int.MaxValue };
    public static ManagedValue Int32ZeroValue { get; } = new() { Int32 = 0 };
    public static ManagedValue Int64MinValue { get; } = new() { Int64 = long.MinValue };
    public static ManagedValue Int64MaxValue { get; } = new() { Int64 = long.MaxValue };
    public static ManagedValue Int64ZeroValue { get; } = new() { Int64 = 0 };
    public static ManagedValue SingleMinValue { get; } = new() { Single = float.MinValue };
    public static ManagedValue SingleMaxValue { get; } = new() { Single = float.MaxValue };
    public static ManagedValue SingleZeroValue { get; } = new() { Single = 0 };
    public static ManagedValue DoubleMinValue { get; } = new() { Double = double.MinValue };
    public static ManagedValue DoubleMaxValue { get; } = new() { Double = double.MaxValue };
    public static ManagedValue DoubleZeroValue { get; } = new() { Double = 0 };
    public static ManagedValue DecimalMinValue { get; } = new() { Decimal = decimal.MinValue };
    public static ManagedValue DecimalMaxValue { get; } = new() { Decimal = decimal.MaxValue };
    public static ManagedValue DecimalZeroValue { get; } = new() { Decimal = 0 };
    public static ManagedValue NullRefValue { get; } = new() { Ref = new(typeof(object), Symbols.Abstract.ScopeKind.Unallocated, null!) };
    public static ManagedValue EmptyStringRefValue { get; } = new() { Ref = new(typeof(string), Symbols.Abstract.ScopeKind.Unallocated, string.Empty) };
}
