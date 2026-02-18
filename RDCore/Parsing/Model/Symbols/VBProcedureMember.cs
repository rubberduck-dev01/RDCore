using RDCore.Parsing.Model.Types;
using RDCore.Parsing.Model.Types.Complex;
using RDCore.Server.ProtocolExtensions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing.Model.Symbols;

internal record class VBProcedureMember : VBTypeMember
{
    protected VBProcedureMember(Uri workspaceUri, SymbolKindExt kind, string name, Accessibility accessibility, Uri parentUri, bool isHidden = false)
        : base(workspaceUri, name, kind, accessibility, parentUri, isHidden)
    {
        ResolvedType = VoidType.VBType;
    }

    protected VBProcedureMember(Uri workspaceUri, SymbolKindExt kind, string name, Accessibility accessibility, Uri parentUri, Range range, Range selectionRange, bool isHidden = false)
        : base(workspaceUri, name, kind, accessibility, parentUri, range, selectionRange, isHidden)
    {
        ResolvedType = VoidType.VBType;
    }

    public VBProcedureMember(Uri workspaceUri, string name, Accessibility accessibility, Uri parentUri, bool isHidden = false)
    : base(workspaceUri, name, SymbolKindExt.Procedure, accessibility, parentUri, isHidden)
    {
        ResolvedType = VoidType.VBType;
    }

    public VBProcedureMember(Uri workspaceUri, string name, Accessibility accessibility, Uri parentUri, Range range, Range selectionRange, bool isHidden = false)
        : base(workspaceUri, name, SymbolKindExt.Procedure, accessibility, parentUri, range, selectionRange, isHidden)
    {
        ResolvedType = VoidType.VBType;
    }
}
