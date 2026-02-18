using RDCore.Parsing.Model.Types;
using RDCore.Server.ProtocolExtensions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing.Model.Symbols;

internal record class VBFunctionMember : VBReturningMember
{
    protected VBFunctionMember(Uri workspaceRoot, string name, Accessibility accessibility, Uri parentUri, bool isHidden = false)
        : base(workspaceRoot, name, accessibility, SymbolKindExt.Function, parentUri, isHidden)
    {
    }

    protected VBFunctionMember(Uri workspaceRoot, string name, Accessibility accessibility, Uri parentUri, Range range, Range selectionRange, bool isHidden = false)
        : base(workspaceRoot, name, accessibility, SymbolKindExt.Function, parentUri, range, selectionRange, isHidden)
    {
    }
}
