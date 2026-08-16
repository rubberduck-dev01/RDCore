using RDCore.Runtime.Execution;
using RDCore.Runtime.Execution.Memory;

namespace RDCore.Tests;

[TestClass]
public class SessionMemoryTests
{
    [TestMethod]
    public void TryAllocate_ReturnsMemoryAddress()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);

        var result1 = sut.TryAllocate(4, out var address1);
        var result2 = sut.TryAllocate(24, out var address2);
        var result3 = sut.TryAllocate(2, out var address3);

        Assert.IsTrue(result1);
        Assert.AreEqual(0, address1.Value);

        Assert.IsTrue(result2);
        Assert.AreEqual(4, address2.Value);

        Assert.IsTrue(result3);
        Assert.AreEqual(28, address3.Value);
    }

    [TestMethod]
    public void TryDeallocate_ReturnsMemoryBlock()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);

        var alloc = sut.TryAllocate(4, out var address1)
            && sut.TryAllocate(24, out var address2) 
            && sut.TryAllocate(2, out var address3);
        if (!alloc)
        {
            Assert.Inconclusive();
        }

        var result = sut.TryDeallocate(address1, out var block1);

        Assert.IsTrue(result);
        Assert.AreEqual(address1, block1.Address);
    }

    [TestMethod]
    public void TryAllocate_ReusesFreeMemory()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);
        _ = sut.TryAllocate(8, out _);

        if (sut.TryAllocate(4, out var address1) &&
            sut.TryDeallocate(address1, out _))
        {
            _ = sut.TryAllocate(4, out var address2);
            Assert.AreEqual(address1, address2);
        }
        else
        {
            Assert.Inconclusive();
        }
    }

    [TestMethod]
    public void SessionMemorySegmentSize32()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);
        Assert.AreEqual(SessionMemorySegment.SegmentSize32, sut.Info.ReservedSegmentBytes);
    }
    [TestMethod]
    public void SessionMemorySegmentSize64()
    {
        var sut = new SessionMemory(new(), PointerSize.x64);
        Assert.AreEqual(SessionMemorySegment.SegmentSize64, sut.Info.ReservedSegmentBytes);
    }

    [TestMethod]
    public void MemoryInfoCommittedBytes_MatchTotalAllocated()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);
        var alloc = 8;

        _ = sut.TryAllocate(alloc, out _);

        Assert.AreEqual(alloc, sut.Info.CommittedBytes);
    }

    [TestMethod]
    public void MemoryInfoAllocatedBytes_MatchTotalAllocated()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);
        var alloc = 8;

        _ = sut.TryAllocate(alloc, out _);

        Assert.AreEqual(alloc, sut.Info.AllocatedBytes);
    }

    [TestMethod]
    public void MemoryInfoReservedBytes_DeallocatedBytesRemainReserved()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);
        var alloc = 8;

        _ = sut.TryAllocate(alloc, out var address);
        sut.TryDeallocate(address, out _);

        Assert.AreEqual(SessionMemorySegment.SegmentSize32, sut.Info.ReservedSegmentBytes);
        Assert.AreEqual(0, sut.Info.AllocatedBytes);
    }

    [TestMethod]
    public void MemoryInfoFreeBytes_MatchDeallocatedBytes()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);
        var alloc = 8;

        _ = sut.TryAllocate(alloc, out var address);
        sut.TryDeallocate(address, out _);

        Assert.AreEqual(alloc, sut.Info.FreeBytes);
        Assert.AreEqual(0, sut.Info.AllocatedBytes);
    }

    [TestMethod]
    public void MemoryInfoLargestFreeBlockSize_MatchLargestDeallocatedBytes()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);
        var allocSmall = 2;
        var allocLarge = 14;

        _ = sut.TryAllocate(allocLarge, out var largeBlockAddress);
        _ = sut.TryAllocate(allocSmall, out _);

        sut.TryDeallocate(largeBlockAddress, out _);

        Assert.AreEqual(allocLarge, sut.Info.FreeBytes);
        Assert.AreEqual(allocSmall, sut.Info.AllocatedBytes);
    }

    [TestMethod]
    public void TryAllocate_SegmentFull_ReservesNewSegment()
    {
        var sut = new SessionMemory(new(), PointerSize.x86);
        var allocSmall = 2;

        var didAllocateSegmentSize = sut.TryAllocate(SessionMemorySegment.SegmentSize32, out _);
        var didAllocateAdditional = sut.TryAllocate(allocSmall, out var smallAllocAddress);

        Assert.IsTrue(didAllocateSegmentSize);
        Assert.IsTrue(didAllocateAdditional);
        Assert.AreEqual(SessionMemorySegment.SegmentSize32 * 2, sut.Info.ReservedSegmentBytes);
        Assert.AreEqual(SessionMemorySegment.SegmentSize32 + allocSmall, sut.Info.AllocatedBytes);
    }
}
