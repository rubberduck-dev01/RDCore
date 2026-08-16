using RDCore.Runtime.Execution.Memory;
using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Model.Values.Bindings;
using System.Diagnostics.CodeAnalysis;

namespace RDCore.Runtime.Execution;

public interface IRuntimeSession
{
    bool Is64Bit { get; }
    ISessionMemoryAllocator Memory { get; }
}
internal sealed class RuntimeSession(SessionMemory memory, SessionSymbols symbols) : IRuntimeSession
{
    public bool Is64Bit { get; init; }
    public ISessionMemoryAllocator Memory { get; init; } = memory;
    public ISessionSymbols Symbols { get; init; } = symbols;
}

public interface ISessionSymbols
{
    bool TryDefine(Symbol symbol);
    bool TryResolve(string name, Symbol scope, out Symbol symbol);
}
internal sealed class SessionSymbols : ISessionSymbols
{
    private readonly Dictionary<Uri, Symbol> _symbolTable = [];
    private readonly Dictionary<string, string> _nameTable = [];

    public bool TryDefine(Symbol symbol)
    {
        // TODO
        return false;
    }
    public bool TryResolve(string name, Symbol scope, out Symbol symbol)
    {
        // TODO
        symbol = default!;
        return false;
    }
}
internal sealed class SessionBindings
{
    private readonly Dictionary<Symbol, IBindingHandle> _globalSymbols = [];
    private readonly Dictionary<Symbol, IBindingHandle> _workspaceSymbols = [];
    private readonly Dictionary<Symbol, IBindingHandle> _staticLocalSymbols = []; // "static" in the VB sense here.

    public bool TryGetValue(Symbol symbol, [MaybeNullWhen(false)][NotNullWhen(true)] out IBindingHandle handle)
    {
        return _staticLocalSymbols.TryGetValue(symbol, out handle)
            || _workspaceSymbols.TryGetValue(symbol, out handle)
            || _globalSymbols.TryGetValue(symbol, out handle);
    }
}
