using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
namespace RDCore.SDK.Model.Values.Interop;

/// <summary>
/// A managed value representing a <c>Currency</c> value.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly record struct ManagedCurrencyInteropValue
{
    private static readonly int ScaleFactor = 10000;

    /// <summary>
    /// Creates a managed value representing a <c>Currency</c> value.
    /// </summary>
    /// <param name="storedValue">The internal 64-bit integer representation of the value.</param>
    public ManagedCurrencyInteropValue(long storedValue)
    {
        StoredValue = storedValue;
    }
    /// <summary>
    /// Creates a managed value representing a <c>Currency</c> value.
    /// </summary>
    /// <param name="scaledValue">The scaled decimal representation of the value.</param>
    /// <remarks>
    /// The internal representation is a 64-bit integer value taking the specified <c>scaledValue</c> multiplied by a <em>scale factor</em> of 10,000.
    /// </remarks>
    public ManagedCurrencyInteropValue(decimal scaledValue)
    {
        StoredValue = Convert.ToInt64(scaledValue * ScaleFactor);
    }

    [FieldOffset(0)] public readonly long StoredValue;
 
    /// <summary>
    /// Gets the scaled decimal representation of the stored value.
    /// </summary>
    public decimal Value => StoredValue / ScaleFactor;
}

[StructLayout(LayoutKind.Explicit)]
public readonly record struct ManagedDecimalInteropValue
{
    public ManagedDecimalInteropValue(decimal storedValue)
    {
        StoredValue = storedValue;
    }

    [FieldOffset(0)] public readonly decimal StoredValue; // FIXME this is a probably-workable but inaccurate representation of a VB Decimal value
}
