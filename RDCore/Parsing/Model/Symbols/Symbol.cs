using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RDCore.Server.ProtocolExtensions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing.Model.Symbols;

internal enum ScopeKind
{
    Unallocated,
    /// <summary>
    /// Symbols from referenced libraries, mostly. Lives in the globals heap.
    /// </summary>
    Global,
    /// <summary>
    /// Procedure level, scoped to the local stack frame.
    /// </summary>
    Local,
    /// <summary>
    /// Module level, lives in the workspace statics heap.
    /// </summary>
    Module,
    /// <summary>
    /// Instance level, lives in the object heap.
    /// </summary>
    Instance,
}

internal abstract record class Symbol : DocumentSymbol
{
    /// <summary>
    /// For symbols representing a user workspace project or a referenced library.
    /// </summary>
    /// <param name="workspaceRoot">A <c>Uri</c> representing the absolute path to the workspace root, or referenced library.</param>
    /// <param name="name">The name of the symbol.</param>
    /// <param name="isUserWorkspace"><c>true</c> when the <c>Uri</c> is the current user workspace root.</param>
    protected Symbol(Uri workspaceRoot, string name, bool isUserWorkspace)
        : this(workspaceRoot, name, SymbolKindExt.Project)
    {
        IsUserWorkspace = isUserWorkspace;
    }

    /// <summary>
    /// For symbols without a <c>Range</c>, e.g. the workspace project's own symbol, symbols from referenced libraries.
    /// </summary>
    /// <param name="workspaceRoot">A <c>Uri</c> representing the absolute path to the library.</param>
    /// <param name="name">The name of the symbol.</param>
    /// <param name="kind">A <c>SymbolKind</c> (extended, LSP-compliant) metadata value describing the kind of symbol.</param>
    /// <param name="parentUri">The <c>Uri</c> of the parent symbol (<c>null</c> for the top-level library symbol).</param>
    protected Symbol(Uri workspaceRoot, string name, SymbolKindExt kind, Uri? parentUri = default)
        : this(workspaceRoot, ScopeKind.Global, name, kind, default!, default!, parentUri)
    {
        IsUserWorkspace = false;
    }

    /// <summary>
    /// For symbols with a <c>Range</c>, i.e. symbols from the workspace project.
    /// </summary>
    protected Symbol(Uri workspaceRoot, ScopeKind scope, string name, SymbolKindExt kind, Range range, Range selectionRange, Uri? parentUri = default)
    {
        Name = name;
        Kind = (SymbolKind)kind;

        Children = [];

        IsUserWorkspace = range is not null && selectionRange is not null;
        ScopeKind = scope;

        ParentUri = parentUri ?? workspaceRoot;
        Uri = CreateUri(ParentUri, name);

        Range = range!;
        SelectionRange = selectionRange ?? Range;
    }

    /// <summary>
    /// A <c>Uri</c> that uniquely identifies the symbol.
    /// </summary>
    public Uri Uri { get; init; }
    /// <summary>
    /// The <c>Uri</c> of the parent symbol.
    /// </summary>
    /// <remarks>
    /// This property is <c>null</c> for a <c>SymbolKind.Project</c> symbol.
    /// </remarks>
    public Uri ParentUri { get; init; } = default!;

    /// <summary>
    /// <c>true</c> if the symbol belongs to the user workspace.
    /// </summary>
    public bool IsUserWorkspace { get; init; }
    /// <summary>
    /// <c>false</c> if the symbol is statically inactive via conditional compilation, <c>true</c> otherwise.
    /// </summary>
    public bool IsActive { get; init; } = true;
    /// <summary>
    /// <c>true</c> if the symbol is semantically <c>Static</c> in the VBA sense.
    /// </summary>
    /// <remarks>
    /// <c>Static</c> locals (explicitly or implicitly so) in VBA retain their value when the scope is later re-entered.
    /// </remarks>
    public bool IsStatic { get; init; } = false;

    public ScopeKind ScopeKind { get; init; }

    public Symbol WithChildren(IEnumerable<Symbol> children) => this with { Children = Container.From(children.OfType<DocumentSymbol>()) };
    public Symbol WithIsActive(bool value) => this with { IsActive = value };

    public static Uri CreateUri(Uri parent, string name)
    {
        var builder = new UriBuilder(parent)
        {
            // If we're already in a fragment, append with a dot. 
            // Otherwise, start the fragment.
            Fragment = string.IsNullOrEmpty(parent.Fragment)
                ? name
                : $"{parent.Fragment.TrimStart('#')}.{name}"
        };
        return builder.Uri;
    }
}
