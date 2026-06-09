using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Model.Symbols.Unbound;
using RDCore.SDK.Model.Values.Abstract;
using RDCore.SDK.Model.Values.Intrinsic;
using RDCore.SDK.Runtime;
using System.Diagnostics.CodeAnalysis;

namespace RDCore.Runtime.Analysis;

public class SimulatedExecutionMemory : IVirtualHeap
{
    private readonly Dictionary<Uri, VBTypedValue> _valueMap;
    private readonly Dictionary<int, Uri> _memoryMap;

    public long Allocate(Uri symbolUri, int size)
    {
        throw new NotImplementedException();
    }

    public long Allocate(Uri symbolUri, VBTypedValue value)
    {
        throw new NotImplementedException();
    }

    public VBObjectValue CreateObject(VBClassModuleSymbol symbol)
    {
        throw new NotImplementedException();
    }

    public void Deallocate(Uri symbolUri)
    {
        throw new NotImplementedException();
    }

    public void Define(Symbol symbol)
    {
        throw new NotImplementedException();
    }

    public VBTypedValue GetValue(Symbol symbol)
    {
        throw new NotImplementedException();
    }

    public Symbol? Resolve(string name, ScopeKind scope, Uri handle)
    {
        throw new NotImplementedException();
    }

    public void SetValue(Symbol symbol, VBTypedValue value)
    {
        throw new NotImplementedException();
    }

    public bool TryRead(long address, [MaybeNullWhen(false), NotNullWhen(true)] out VBTypedValue? value)
    {
        throw new NotImplementedException();
    }
}