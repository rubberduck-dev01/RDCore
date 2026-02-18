using RDCore.Parsing.Model.Types;
using RDCore.Server.ProtocolExtensions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing.Model.Symbols;

internal interface IVBProperty
{
    string Name { get; }
}

internal record class VBPropertyGetMember : VBReturningMember, IVBProperty
{
    public VBPropertyGetMember(Uri workspaceRoot, string name, Accessibility accessibility, Uri parentUri, bool isHidden = false)
        : base(workspaceRoot, name, accessibility, SymbolKindExt.Property, parentUri, isHidden)
    {
    }

    public VBPropertyGetMember(Uri workspaceRoot, string name, Accessibility accessibility, Uri parentUri, Range range, Range selectionRange, bool isHidden = false)
        : base(workspaceRoot, name, accessibility, SymbolKindExt.Property, parentUri, range, selectionRange, isHidden)
    {
    }
}
