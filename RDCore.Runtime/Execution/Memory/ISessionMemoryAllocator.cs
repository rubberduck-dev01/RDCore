using RDCore.SDK.Runtime.Shared;

namespace RDCore.Runtime.Execution.Memory;

public interface ISessionMemoryAllocator
{
    /// <summary>
    /// Allocates the specified number of bytes in the memory space of this session.
    /// </summary>
    /// <param name="size">The desired size of the allocation.</param>
    /// <param name="address">The start of the allocated address space.</param>
    /// <returns><c>true</c> if the specified number of bytes can be allocated, <c>false</c> otherwise.</returns>
    bool TryAllocate(int size, out MemoryAddress address);
    bool TryDeallocate(MemoryAddress address, out SessionMemoryBlock block);
    SessionMemoryInfo Info { get; }
}
