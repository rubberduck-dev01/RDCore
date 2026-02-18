using RDCore.Parsing.Model.Types.Complex;
using RDCore.Server.ProtocolExtensions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing.Model.Types;

internal abstract record class VBReturningMember : VBTypeMember
{
    protected VBReturningMember(Uri workspaceRoot, string name, Accessibility accessibility, SymbolKindExt kind, Uri parentUri, bool isHidden = false)
        : base(workspaceRoot, name, kind, accessibility, parentUri, isHidden)
    {
    }
    protected VBReturningMember(Uri workspaceRoot, string name, Accessibility accessibility, SymbolKindExt kind, Uri parentUri, Range range, Range selectionRange, bool isHidden = false)
        : base(workspaceRoot, name, kind, accessibility, parentUri, range, selectionRange, isHidden)
    {
    }
}
