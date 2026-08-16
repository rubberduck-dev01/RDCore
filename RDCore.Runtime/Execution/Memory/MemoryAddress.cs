namespace RDCore.Runtime.Execution.Memory;

/// <summary>
/// Represents an address in program/session memory.
/// </summary>
/// <param name="Value">The address value.</param>
public record struct MemoryAddress(int Value)
{
    public static MemoryAddress operator +(MemoryAddress address, int offset) => new(address.Value + offset);
    public static MemoryAddress operator -(MemoryAddress address, int offset) => new(address.Value - offset);
    public override readonly string ToString() => Value.ToString("X");
}
