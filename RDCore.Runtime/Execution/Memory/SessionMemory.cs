namespace RDCore.Runtime.Execution.Memory;

internal sealed class SessionMemory : ISessionMemoryAllocator
{
    private readonly FreeListManager _freeLists;
    private readonly Stack<SessionMemorySegment> _segments = new(4);
    private readonly int _size;

    public SessionMemory(FreeListManager freeLists, PointerSize pointerSize)
    {
        _freeLists = freeLists;
        _size = pointerSize == PointerSize.x86 ? SessionMemorySegment.SegmentSize32 : SessionMemorySegment.SegmentSize64;
        _segments.Push(GetNewReservedSegment(new(0)));
        PointerSize = pointerSize;
    }

    private SessionMemorySegment GetNewReservedSegment(MemoryAddress address) => new(address, _size, PointerSize);

    public PointerSize PointerSize { get; }

    public SessionMemoryInfo Info
    {
        get
        {
            var totalReserved = 0;
            var totalAllocated = 0;
            var totalCommitted = 0;
            var totalFreeList = 0;

            foreach (var segment in _segments)
            {
                totalReserved += segment.Info.ReservedSegmentBytes;
                totalAllocated += segment.Info.AllocatedBytes;
                totalCommitted += segment.Info.CommittedBytes;
                totalFreeList += segment.Info.FreeBytes;
            }

            return new(totalReserved, totalAllocated, totalCommitted, totalFreeList);
        }
    }

    public bool TryAllocate(int size, out MemoryAddress address)
    {
        var currentSegment = _segments.Peek();
        if (size > currentSegment.Size)
        {
            address = default;
            return false;
        }

        if (_freeLists.TryGetFreeListBlock(size, out var freeBlock, out var segment))
        {
            if (freeBlock.Value.Address.Value >= segment.Address.Value
                && freeBlock.Value.Address.Value < segment.NextSegment.Value)
            {
                address = segment.Allocate(freeBlock.Value);
                return true;
            }
        }

        if (!currentSegment.TryAllocate(size, out address))
        {
            currentSegment = GetNewReservedSegment(currentSegment.NextSegment);
            _segments.Push(currentSegment);
        }

        return currentSegment.TryAllocate(size, out address);
    }

    public bool TryDeallocate(MemoryAddress address, out SessionMemoryBlock block)
    {
        // NOTE: deallocation does not check if the segment is left empty; this is intentional.
        var currentSegment = _segments.Peek();
        if (currentSegment.TryDeallocate(address, out block))
        {
            _freeLists.Add(block, currentSegment);
            return true;
        }
        return false;
    }
}
