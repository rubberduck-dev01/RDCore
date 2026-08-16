namespace RDCore.Runtime.Execution.Memory;

public record struct SessionMemoryInfo(
    int ReservedSegmentBytes, 
    int AllocatedBytes, 
    int CommittedBytes,
    int FreeBytes)
{
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
        FreeBytes = FreeBytes + bytes
    };
}
