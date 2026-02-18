using RDCore.Parsing.Model.Symbols;

namespace RDCore.Runtime;

internal record ScopeContext
{
    public ScopeContext(Symbol scopeSymbol, ScopeContext? parent = null)
    {
        ScopeSymbol = scopeSymbol;
        Parent = parent;
    }

    // These would be resolved from annotations or module headers
    public bool OptionStrict => GetOption(s => s.OptionStrict);
    public bool OptionExplicit => GetOption(s => s.OptionExplicit);
    public bool OptionCompareBinary => GetOption(s => s.OptionCompareBinary);

    public Symbol ScopeSymbol { get; }
    public ScopeContext? Parent { get; }

    private bool GetOption(Func<ScopeContext, bool> selector)
        => selector(this) || (Parent?.GetOption(selector) ?? false);
}
