using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using RDCore.SDK.Model.Symbols.Abstract;
namespace RDCore.SDK.Model.Values.Interop;

/// <summary>
/// A managed value representing a reference.
/// </summary>
/// <param name="ManagedType">The managed type representing the value.</param>
/// <param name="ValueAlloc">The allocation scope of the value.</param>
/// <param name="Value">The value.</param>
public readonly record struct ManagedInteropReference(Type ManagedType, ScopeKind ValueAlloc, object Value)
{
    public static readonly ManagedInteropReference NullRef = new(typeof(Object), ScopeKind.Unallocated, null!);
    public static readonly ManagedInteropReference NullStringRef = new(typeof(string), ScopeKind.Unallocated, null!);
    public static readonly ManagedInteropReference EmptyStringRef = new(typeof(string), ScopeKind.Unallocated, string.Empty);
}