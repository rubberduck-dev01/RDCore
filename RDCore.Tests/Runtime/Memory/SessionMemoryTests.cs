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
}
