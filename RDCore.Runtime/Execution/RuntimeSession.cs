using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Model.Values.Bindings;
using System.Diagnostics.CodeAnalysis;

namespace RDCore.Runtime.Execution;

public interface IRuntimeSession
{
    bool Is64Bit { get; }
    ISessionMemoryAllocator Memory { get; }
}
public interface ISessionSymbols
{
    bool TryDefine(Symbol symbol);
    bool TryResolve(string name, Symbol scope, out Symbol symbol);
}
internal sealed class RuntimeSession(SessionMemory memory, SessionSymbols symbols) : IRuntimeSession
{
    public bool Is64Bit { get; init; }
    public ISessionMemoryAllocator Memory { get; init; } = memory;
    public ISessionSymbols Symbols { get; init; } = symbols;
}

internal sealed class SessionSymbols : ISessionSymbols
{
    private readonly Dictionary<Uri, Symbol> _symbolTable = [];
    private readonly Dictionary<string, string> _nameTable = [];

    public bool TryDefine(Symbol symbol)
    {
        // TODO
        return false;
    }
    public bool TryResolve(string name, Symbol scope, out Symbol symbol)
    {
        // TODO
        symbol = default!;
        return false;
    }
}
internal sealed class SessionBindings
{
    private readonly Dictionary<Symbol, IBindingHandle> _globalSymbols = [];
    private readonly Dictionary<Symbol, IBindingHandle> _workspaceSymbols = [];
    private readonly Dictionary<Symbol, IBindingHandle> _staticLocalSymbols = []; // "static" in the VB sense here.

    public bool TryGetValue(Symbol symbol, [MaybeNullWhen(false)][NotNullWhen(true)] out IBindingHandle handle)
    {
        return _staticLocalSymbols.TryGetValue(symbol, out handle)
            || _workspaceSymbols.TryGetValue(symbol, out handle)
            || _globalSymbols.TryGetValue(symbol, out handle);
    }
}

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

internal class FreeListManager
{
    private static readonly int _smallListSize = 8;

    private readonly Dictionary<SessionMemoryBlock, SessionMemorySegment> _segmentMap = [];
    private readonly SortedList<int, SessionMemoryBlock> _freeSmallBlocks = [];
    private readonly SortedList<int, SessionMemoryBlock> _freeLargeBlocks = [];

    public void Add(SessionMemoryBlock block, SessionMemorySegment segment)
    {
        var freeList = block.Size <= _smallListSize ? _freeSmallBlocks : _freeLargeBlocks;
        freeList.Add(block.Size, block);
        _segmentMap.Add(block, segment);
        // merge contiguous free blocks?
    }

    public bool TryGetFreeListBlock(int size, [MaybeNullWhen(false)][NotNullWhen(true)] out SessionMemoryBlock? block, [MaybeNullWhen(false)][NotNullWhen(true)] out SessionMemorySegment? segment)
    {
        var freeList = size <= _smallListSize ? _freeSmallBlocks : _freeLargeBlocks;
        if (freeList[freeList.Count - 1].Size >= size)
        {
            // free-list is usable, so we use the smallest available block that fits
            for (var i = 0; i < freeList.Count; i++)
            {
                if (freeList[i].Size >= size)
                {
                    block = freeList[i];  // ideal case: no fragmentation

                    freeList.RemoveAt(i); // this block is no longer free
                    _segmentMap.Remove(block.Value, out segment!);

                    if (freeList[i].Size > size)
                    {
                        // free block is larger than the size we need; this leaves a fragment block behind
                        var fragment = new SessionMemoryBlock(block.Value.Address + (size - 1), block.Value.Size - size);
                        block = new SessionMemoryBlock(block.Value.Address, size);

                        // add the fragment as a new free block
                        if (fragment.Size < _smallListSize)
                        {
                            _freeSmallBlocks.Add(fragment.Size, fragment);
                            _segmentMap.Add(fragment, segment);
                        }
                        else
                        {
                            _freeLargeBlocks.Add(fragment.Size, fragment);
                            _segmentMap.Add(fragment, segment);
                        }
                    }
                    return true;
                }
            }
        }
        block = default;
        segment = default;
        return false;
    }
}

public enum PointerSize
{
    x86 = 4,
    x64 = 8
}

/// <summary>
/// Represents a reserved segment of contiguous managed memory.
/// </summary>
/// <param name="Address">The start address.</param>
/// <param name="PointerSize">The size of an object pointer, in <strong>bytes</strong>.</param>
internal record class SessionMemorySegment : ISessionMemoryAllocator
{
    public static readonly int SegmentSize32 = 4096;
    public static readonly int SegmentSize64 = 8192;

    public SessionMemorySegment(MemoryAddress address, int size, PointerSize pointerSize)
    {
        Address = address;
        Size = size;
        PointerSize = pointerSize;

        _currentAddress = address;
        _nextSegmentAddress = address + (pointerSize == PointerSize.x86 ? SegmentSize32 : SegmentSize64);
    }
    public MemoryAddress Address { get; }
    public MemoryAddress NextSegment { get; }

    public int Size { get; }
    public PointerSize PointerSize { get; }


    private readonly Dictionary<MemoryAddress, SessionMemoryBlock> _memoryMap = [];

    private MemoryAddress _currentAddress;
    private readonly MemoryAddress _nextSegmentAddress;
    private MemoryAddress Advance(int size) => _currentAddress += size;

    private SessionMemoryInfo _info;
    public SessionMemoryInfo Info => _info;

    public bool TryAllocate(int size, out MemoryAddress address)
    {
        if (_currentAddress.Value + size >= _nextSegmentAddress.Value)
        {
            // segment is full
            address = default;
            return false;
        }

        address = Allocate(new SessionMemoryBlock(_currentAddress, size));
        Advance(size);

        return true;
    }

    internal MemoryAddress Allocate(SessionMemoryBlock block)
    {
        _memoryMap[block.Address] = block;

        _info = _info.WithAllocated(block.Size);
        return block.Address;
    }

    public bool TryDeallocate(MemoryAddress address, out SessionMemoryBlock block)
    {
        if (_memoryMap.Remove(address, out block))
        {
            _info = _info.WithAllocated(-block.Size);
            return true;
        }
        return false;
    }
}
/// <summary>
/// Represents a block of memory space allocated inside a <em>memory segment</em>.
/// </summary>
/// <param name="Address">The start address.</param>
/// <param name="Size">The size of the block in bytes.</param>
public record struct SessionMemoryBlock(MemoryAddress Address, int Size);

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

    private SessionMemorySegment GetNewReservedSegment(MemoryAddress address) 
        => new(address, _size, PointerSize);

    public PointerSize PointerSize { get; }

    public SessionMemoryInfo Info
    {
        get
        {
            var totalReserved = 0;
            var totalAllocated = 0;
            var totalCommitted = 0;
            var totalFreeList = 0;
            var totalFragmented = 0;

            foreach (var segment in _segments)
            {
                totalReserved += segment.Info.ReservedSegmentBytes;
                totalAllocated += segment.Info.AllocatedBytes;
                totalCommitted += segment.Info.CommittedBytes;
                totalFreeList += segment.Info.FreeListBytes;
                totalFragmented += segment.Info.FragmentedBytes;
            }

            return new(totalReserved, totalAllocated, totalCommitted, totalFreeList, totalFragmented);
        }
    }

    public bool TryAllocate(int size, out MemoryAddress address)
    {
        var currentSegment = _segments.Peek();
        if (size > currentSegment.Size)
        {
            // FIXME 
            // edge case that eventually needs to be handled: very large objects (mostly arrays) could be larger than a segment.
            // the current logic does not allow allocating blocks across segments.
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
