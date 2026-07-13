namespace RDCore.SDK.Model.Values.Interop;

public enum ManagedInteropWrapperKind
{
    Value,
    Reference,
    Variant,
}

/// <summary>
/// A wrapper encapsulating any kind of <em>data value</em>.
/// </summary>
public readonly record struct ManagedInteropWrapper
{
    public ManagedInteropWrapper(ManagedInteropValue value)
    {
        InteropValue = value;
        Kind = ManagedInteropWrapperKind.Value;
    }
    public ManagedInteropWrapper(ManagedInteropReference reference)
    {
        InteropReference = reference;
        Kind = ManagedInteropWrapperKind.Reference;
    }
    public ManagedInteropWrapper(ManagedInteropVariant variant)
    {
        InteropVariant = variant;
        Kind = ManagedInteropWrapperKind.Variant;
    }

    public ManagedInteropWrapperKind Kind { get; init; }

    public ManagedInteropValue? InteropValue { get; init; }
    public ManagedInteropReference? InteropReference { get; init; }
    public ManagedInteropVariant? InteropVariant { get; init; }
}
