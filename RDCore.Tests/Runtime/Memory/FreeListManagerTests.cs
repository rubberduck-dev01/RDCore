using RDCore.Runtime.Execution;
using RDCore.Runtime.Execution.Memory;

namespace RDCore.Tests;

[TestClass]
public class FreeListManagerTests
{
    [TestMethod]
    public void EmptyLists_TryGetFreeListBlock_IsFalse()
    {
        var sut = new FreeListManager();

        var result = sut.TryGetFreeListBlock(4, out var freeBlock, out var freeBlockSegment);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Add_SmallBlock_CanGetSmallFreeBlock()
    {
        var sut = new FreeListManager();

        var segment = new SessionMemorySegment(new MemoryAddress(0), 2048, PointerSize.x86);
        var block = new SessionMemoryBlock(new MemoryAddress(0), 4); // <= 8
        sut.Add(block, segment);

        var result = sut.TryGetFreeListBlock(4, out var freeBlock, out var freeBlockSegment);

        Assert.IsTrue(result);
        Assert.IsNotNull(freeBlock);
        Assert.IsNotNull(freeBlockSegment);
        Assert.AreEqual(block, freeBlock);
        Assert.AreEqual(segment, freeBlockSegment);
    }

    [TestMethod]
    public void AddLargeBlock_TryGetSmallFreeBlock_IsFalse()
    {
        var sut = new FreeListManager();

        var segment = new SessionMemorySegment(new MemoryAddress(0), 2048, PointerSize.x86);
        var block = new SessionMemoryBlock(new MemoryAddress(0), 24); // > 8 
        sut.Add(block, segment);

        var result = sut.TryGetFreeListBlock(4, out var freeBlock, out var freeBlockSegment);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void AddLargeBlock_TryGetLargeFreeBlock_IsTrue()
    {
        var sut = new FreeListManager();

        var segment = new SessionMemorySegment(new MemoryAddress(0), 2048, PointerSize.x86);
        var block = new SessionMemoryBlock(new MemoryAddress(0), 24); // > 8 
        sut.Add(block, segment);

        var result = sut.TryGetFreeListBlock(14, out var freeBlock, out var freeBlockSegment);

        Assert.IsTrue(result);
        Assert.AreEqual(block.Address, freeBlock!.Value.Address);
    }
}
