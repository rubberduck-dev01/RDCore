using System.Diagnostics.CodeAnalysis;

namespace RDCore.Runtime.Execution.Memory;

internal record class FreeBlocksList
{
    private static readonly SessionMemoryBlockSizeComparer _comparer = new();
    private readonly List<SessionMemoryBlock> _blocks = new(32);

    private int _largestBlockSize = 0;
    public int LargestBlockSize => _largestBlockSize;
    public SessionMemoryBlock this[int index] => _blocks[index];
    public int Count => _blocks.Count;
    internal bool Contains(MemoryAddress address) => _blocks.Any(block => block.Address == address);

    private int _totalSize = 0;
    public int TotalSize => _totalSize;

    public bool TryGetFreeBlock(int size, [NotNullWhen(true)][MaybeNullWhen(false)] out SessionMemoryBlock? block)
    {
        block = default;
        if (size > _largestBlockSize)
        {
            return false;
        }

        for (var i = 0; i < _blocks.Count; i++)
        {
            if (_blocks[i].Size <= size)
            {
                block = _blocks[i];
                continue;
            }
            else if (_blocks[i].Size > size)
            {
                break;
            }
        }

        return (block?.Size ?? 0) == size;
    }

    public void RemoveAt(int index)
    {
        var block = _blocks[index];
        _totalSize -= block.Size;
        _blocks.RemoveAt(index);
    }

    public void Add(SessionMemoryBlock block)
    {
        if (_blocks.Count == 0)
        {
            _blocks.Add(block);
            _largestBlockSize = block.Size;
            _totalSize += block.Size;
            return;
        }

        var index = 0;
        SessionMemoryBlock previous = _blocks[0];

        while (index < _blocks.Count && _comparer.Compare(_blocks[index], block) <= 0)
        {
            previous = _blocks[index];
            index++;
        }

        if (previous.Address.Value + previous.Size == block.Address.Value)
        {
            _blocks.Remove(previous);

            var merged = new SessionMemoryBlock(previous.Address, previous.Size + block.Size);
            if (_blocks.Count > index)
            {
                _blocks.Insert(index, merged);
            }
            else
            {
                _blocks.Add(merged);
            }
        }
        else
        {
            _blocks.Insert(index, block);
        }

        _totalSize += block.Size;
    }
}
