using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Runtime;
using RDCore.SDK.Semantics.Runtime.Abstract;

namespace RDCore.Runtime.Analysis;

public class SimulatedExecutionContext(bool is64bit, IVirtualHeap memory) : IVBExecutionContext
{
    public bool Is64Bit { get; } = is64bit;
    public IVirtualHeap Memory => memory;

    private readonly Stack<IIndexedStackFrame> _frames = [];

    public RuntimeSemanticsEvaluationResult Call(VBTypeMemberSymbol symbol)
    {
        throw new NotImplementedException();
    }
}
