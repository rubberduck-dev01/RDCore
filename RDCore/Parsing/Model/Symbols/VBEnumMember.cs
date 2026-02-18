using RDCore.Parsing.Model.Types;
using RDCore.Server.ProtocolExtensions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace RDCore.Parsing.Model.Symbols;

internal record class VBEnumMember : VBReturningMember
{
    public VBEnumMember(Uri workspaceRoot, string name, Uri parentUri, BoundExpression valueExpression, bool isHidden = false)
        : base(workspaceRoot, name, Accessibility.Public, SymbolKindExt.EnumMember, parentUri, isHidden)
    {
        ResolvedType = VBLongType.TypeInfo;
        ValueExpression = valueExpression;
    }

    public VBEnumMember(Uri workspaceRoot, string name, Uri parentUri, BoundExpression? valueExpression, Range range, Range selectionRange, bool isHidden = false)
        : base(workspaceRoot, name, Accessibility.Public, SymbolKindExt.EnumMember, parentUri, range, selectionRange, isHidden)
    {
        ResolvedType = VBLongType.TypeInfo;
        ValueExpression = valueExpression;
    }

    public BoundExpression? ValueExpression { get; init; }
}
