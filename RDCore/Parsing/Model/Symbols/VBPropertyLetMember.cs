using RDCore.Server.ProtocolExtensions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing.Model.Symbols;

internal record class VBPropertyLetMember : VBProcedureMember, IVBProperty
{
    public VBPropertyLetMember(Uri workspaceUri, string name, Accessibility accessibility, Uri parentUri, bool isHidden = false)
        : base(workspaceUri, SymbolKindExt.Property, name, accessibility, parentUri, isHidden)
    {
    }

    public VBPropertyLetMember(Uri workspaceUri, string name, Accessibility accessibility, Uri parentUri, Range range, Range selectionRange, bool isHidden = false)
        : base(workspaceUri, SymbolKindExt.Property, name, accessibility, parentUri, range, selectionRange, isHidden)
    {
    }
}
