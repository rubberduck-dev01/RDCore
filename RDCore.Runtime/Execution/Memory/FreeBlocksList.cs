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

    public void RemoveAt(int index)
    {
        var block = _blocks[index];
        _blocks.RemoveAt(index);
    }

    public void Add(SessionMemoryBlock block)
    {
        if (_blocks.Count == 0)
        {
            _blocks.Add(block);
            _largestBlockSize = block.Size;
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
                _largestBlockSize = merged.Size;
                _blocks.Add(merged);
            }
        }
        else
        {
            _blocks.Insert(index, block);
        }
    }
}
