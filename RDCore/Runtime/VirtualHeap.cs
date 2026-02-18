using RDCore.Parsing.Model.Symbols;
using RDCore.Parsing.Model.Values;

namespace RDCore.Runtime;

internal class VirtualHeap(VBObjectValue context)
{
    private readonly Stack<Dictionary<Symbol, VBTypedValue>> _stackFrames = [];

    private readonly Dictionary<Symbol, VBTypedValue> _globalHeap = [];
    private readonly Dictionary<Symbol, VBTypedValue> _workspaceHeap = [];
    private readonly Dictionary<Symbol, VBTypedValue> _staticLocalsHeap = [];
    private readonly Dictionary<VBObjectValue, Dictionary<Symbol, VBTypedValue>> _objectHeap = [];

    public VBTypedValue GetValue(Symbol symbol)
    {
        return symbol.ScopeKind switch
        {
            ScopeKind.Global => _globalHeap[symbol],
            ScopeKind.Module => _workspaceHeap[symbol],
            ScopeKind.Instance => _objectHeap[context][symbol],
            ScopeKind.Local => symbol.IsStatic
                ? _staticLocalsHeap[symbol]
                : _stackFrames.Peek()[symbol],
            _ => throw new InvalidOperationException()
        };
    }

    public void SetValue(Symbol symbol, VBTypedValue value)
    {
        var heap = symbol.ScopeKind switch
        {
            ScopeKind.Global => _globalHeap,
            ScopeKind.Module => _workspaceHeap,
            ScopeKind.Instance => _objectHeap[context],
            ScopeKind.Local => symbol.IsStatic
                ? _staticLocalsHeap
                : _stackFrames.Peek(),
            _ => throw new InvalidOperationException()
        };

        heap[symbol] = value;
    }
}
