using RDCore.Parsing.Model.Symbols;
using RDCore.Parsing.Model.Values;

namespace RDCore.Runtime;

internal class VirtualHeap(VBExecutionContext context)
{
    private static readonly long _offset = 0x1000;
    //private static readonly long _addressSpace = 0x10FF; // TODO update from MS-VBAL, if specified

    private readonly VBExecutionContext _context = context;
    private readonly Stack<Dictionary<Symbol, VBTypedValue>> _stackFrames = [];

    private readonly Dictionary<Symbol, VBTypedValue> _globalHeap = [];
    private readonly Dictionary<Symbol, VBTypedValue> _workspaceHeap = [];
    private readonly Dictionary<Symbol, VBTypedValue> _staticLocalsHeap = [];
    //private readonly Dictionary<VBObjectValue, Dictionary<Symbol, VBTypedValue>> _objectHeap = [];

    private long _nextObject = _offset;

    private readonly Dictionary<VBObjectValue, VBLongPtrValue> _objPtrMap = [];
    private readonly Dictionary<VBTypedValue, VBLongPtrValue> _varPtrMap = [];
    private readonly Dictionary<VBStringValue, VBLongPtrValue> _strPtrMap = [];

    private readonly Dictionary<Uri, VBLongPtrValue> _allocatedSymbols = [];

    public VBLongPtrValue ObjPtr(VBObjectValue obj) => _objPtrMap.TryGetValue(obj, out var pointer) && pointer is VBLongPtrValue value ? value : VBLongPtrValue.Zero;
    public VBTypedValue VarPtr(VBTypedValue local) => _varPtrMap.TryGetValue(local, out var pointer) && pointer is VBLongPtrValue value ? value : VBLongPtrValue.Zero;
    public VBTypedValue StrPtr(VBStringValue str) => _strPtrMap.TryGetValue(str, out var pointer) && pointer is VBLongPtrValue value ? value : VBLongPtrValue.Zero;
    public VBObjectValue CreateObject(ClassModuleSymbol symbol)
    {
        var address = _nextObject;
        Interlocked.Add(ref _nextObject, VBLongPtrValue.BitnessAwarePtrSize);

        var ptr = new VBLongPtrValue(symbol) { NumericValue = address };
        var obj = new VBObjectValue(symbol, ptr);

        _objPtrMap[obj] = ptr;

        // TODO: Shift 2 - Invoke _monitors.ForEach(m => m.OnAllocate(...))

        return obj;
    }

    public VBTypedValue GetValue(Symbol symbol)
    {
        return symbol.ScopeKind switch
        {
            ScopeKind.Global => _globalHeap[symbol],
            ScopeKind.Module => _workspaceHeap[symbol],
            ScopeKind.Local => symbol.Get(SymbolProperties.IsStatic) ? _staticLocalsHeap[symbol] : _stackFrames.Peek()[symbol],
            _ => throw new InvalidOperationException()
        };
    }

    public void SetValue(Symbol symbol, VBTypedValue value)
    {
        var heap = symbol.ScopeKind switch
        {
            ScopeKind.Global => _globalHeap,
            ScopeKind.Module => _workspaceHeap,
            ScopeKind.Local => symbol.Get(SymbolProperties.IsStatic) ? _staticLocalsHeap : _stackFrames.Peek(),
            _ => throw new InvalidOperationException()
        };

        heap[symbol] = value;
    }

    public VBLongPtrValue Allocate(Uri symbolUri, int size)
    {
        var address = new VBLongPtrValue { NumericValue = _nextObject };

        var step = VBLongPtrValue.BitnessAwarePtrSize;
        Interlocked.Add(ref _nextObject, Math.Max(size, step));

        _allocatedSymbols[symbolUri] = address;

        // TODO fire IHeapMonitor.OnAllocate here
        return address;
    }

    public bool Deallocate(Uri symbolUri)
    {
        return _allocatedSymbols.Remove(symbolUri);
    }
}
