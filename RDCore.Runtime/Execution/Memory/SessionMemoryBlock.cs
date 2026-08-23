using RDCore.SDK.Runtime.Shared;

namespace RDCore.Runtime.Execution.Memory;

/// <summary>
/// Represents a block of memory space allocated inside a <em>memory segment</em>.
/// </summary>
/// <param name="Address">The start address.</param>
/// <param name="Size">The size of the block in bytes.</param>
public record struct SessionMemoryBlock(MemoryAddress Address, int Size);
