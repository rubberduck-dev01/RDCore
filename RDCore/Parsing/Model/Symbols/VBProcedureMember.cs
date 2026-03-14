using RDCore.Parsing.Model.Types;
using RDCore.Parsing.Model.Types.Complex;
using RDCore.Server.ProtocolExtensions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing.Model.Symbols;

internal record class VBProcedureMember : VBTypeMemberSymbol
{
    protected VBProcedureMember(Uri workspaceUri, SymbolKindExt kind, string name, Accessibility accessibility, Uri parentUri)
        : base(workspaceUri, name, kind, accessibility, parentUri)
    {
        ResolvedType = VoidType.VBType;
    }

    protected VBProcedureMember(Uri workspaceUri, SymbolKindExt kind, string name, Accessibility accessibility, Uri parentUri, Range range, Range selectionRange)
        : base(workspaceUri, name, kind, accessibility, parentUri, range, selectionRange)
    {
        ResolvedType = VoidType.VBType;
    }

    public VBProcedureMember(Uri workspaceUri, string name, Accessibility accessibility, Uri parentUri)
    : base(workspaceUri, name, SymbolKindExt.Procedure, accessibility, parentUri)
    {
        ResolvedType = VoidType.VBType;
    }

    public VBProcedureMember(Uri workspaceUri, string name, Accessibility accessibility, Uri parentUri, Range range, Range selectionRange)
        : base(workspaceUri, name, SymbolKindExt.Procedure, accessibility, parentUri, range, selectionRange)
    {
        ResolvedType = VoidType.VBType;
    }
}
