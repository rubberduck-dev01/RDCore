using RDCore.Runtime.Execution.Memory;
using RDCore.SDK.Runtime.Shared;

namespace RDCore.Tests;

[TestClass]
public class FreeBlocksListTests
{
    [TestMethod]
    public void Add_IncrementsCount()
    {
        var sut = new FreeBlocksList();

        sut.Add(new SessionMemoryBlock(new MemoryAddress(42), 4));
        sut.Add(new SessionMemoryBlock(new MemoryAddress(10), 2));

        Assert.AreEqual(2, sut.Count);
    }

    [TestMethod]
    public void Add_MergesContiguous()
    {
        var sut = new FreeBlocksList();

        sut.Add(new SessionMemoryBlock(new MemoryAddress(0), 4));
        sut.Add(new SessionMemoryBlock(new MemoryAddress(4), 2));
        sut.Add(new SessionMemoryBlock(new MemoryAddress(6), 8));

        Assert.AreEqual(1, sut.Count);
    }

    [TestMethod]
    public void Add_SortsBySize()
    {
        var sut = new FreeBlocksList();

        var block1 = new SessionMemoryBlock(new MemoryAddress(10), 2);
        var block2 = new SessionMemoryBlock(new MemoryAddress(20), 4);
        var block3 = new SessionMemoryBlock(new MemoryAddress(30), 8);

        sut.Add(block3);
        sut.Add(block1);
        sut.Add(block2);

        Assert.AreEqual(block1, sut[0]);
        Assert.AreEqual(block2, sut[1]);
        Assert.AreEqual(block3, sut[2]);
    }

    [TestMethod]
    public void RemoveAt_RemovesBlock()
    {
        var sut = new FreeBlocksList();

        var block1 = new SessionMemoryBlock(new MemoryAddress(10), 2);
        var block2 = new SessionMemoryBlock(new MemoryAddress(20), 4);
        var block3 = new SessionMemoryBlock(new MemoryAddress(30), 8);

        sut.Add(block1);
        sut.Add(block2);
        sut.Add(block3);

        sut.RemoveAt(1);

        Assert.AreEqual(2, sut.Count);
        Assert.IsFalse(sut.Contains(block2.Address));
        Assert.IsTrue(sut.Contains(block1.Address));
        Assert.IsTrue(sut.Contains(block3.Address));
    }
}
