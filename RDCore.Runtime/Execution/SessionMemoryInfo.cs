namespace RDCore.Runtime.Execution;

/// <summary>
/// 
/// </summary>
/// <param name="ReservedSegmentBytes">The number of bytes <em>reserved</em> for the session memory.</param>
/// <param name="AllocatedBytes">The number of bytes <em>allocated</em> in session memory.</param>
/// <param name="CommittedBytes">The number of bytes committed (allocated, free, or fragmented)</param>
/// <param name="FreeListBytes">The number of bytes currently held by memory blocks in free-list storage.</param>
public record struct SessionMemoryInfo(
    int ReservedSegmentBytes, 
    int AllocatedBytes, 
    int CommittedBytes,
    int FreeListBytes,
    int FragmentedBytes)
{
    public readonly double FragmentationPercent => CommittedBytes == 0 ? 0 : FragmentedBytes / CommittedBytes;
    public readonly double AvailablePercent => (ReservedSegmentBytes - AllocatedBytes - FreeListBytes - FragmentedBytes) / ReservedSegmentBytes;

    public SessionMemoryInfo WithReserved(int bytes) => this with 
    {
        ReservedSegmentBytes = ReservedSegmentBytes + bytes
    };
    public SessionMemoryInfo WithAllocated(int bytes) => this with
    {
        AllocatedBytes = AllocatedBytes + bytes
    };
    public SessionMemoryInfo WithCommitted(int bytes) => this with 
    { 
        CommittedBytes = CommittedBytes + bytes 
    };
    public SessionMemoryInfo WithFreeList(int bytes) => this with
    {
        FreeListBytes = FreeListBytes + bytes
    };
    public SessionMemoryInfo WithFragmented(int bytes) => this with
    {
        FragmentedBytes = FragmentedBytes + bytes
    };
}
