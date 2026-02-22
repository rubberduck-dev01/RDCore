using RDCore.Parsing.Model.Symbols;
using RDCore.Parsing.Model.Values;
using RDCore.Server.ProtocolExtensions;
using System.Collections.Immutable;

namespace RDCore.Parsing.Model.Types.Complex;

internal interface IVBInferableType
{
    ImmutableHashSet<VBType> CandidateTypes { get; init; }
    IVBInferableType WithCandidateType(VBType vbType);
}

internal interface IVBDeferrableType : IVBInferableType
{
    IVBMemberOwnerType? DeferredVBType { get; init; }
    VBDeferredType WithDeferredVBType(IVBMemberOwnerType vbType);
}

internal interface IVBDeferrableTypeMember : IVBInferableType
{
    VBType? DeferredVBType { get; init; }
    VBDeferredTypeMember WithDeferredVBType(VBType vbType);
}

internal abstract record class VBDeferredType : VBType, IVBDeferrableType
{
    public VBDeferredType(string name, Uri uri)
        : base(typeof(object), name, isUserDefined: true, isHidden: true)
    {
        Uri = uri;
    }

    public Uri Uri { get; init; }

    public ImmutableHashSet<VBDeferredTypeMember> Members { get; init; } = [];
    public VBDeferredType WithMembers(IEnumerable<VBDeferredTypeMember> members) => this with { Members = [.. members] };

    public IVBMemberOwnerType? DeferredVBType { get; init; }
    public VBDeferredType WithDeferredVBType(IVBMemberOwnerType vbType) => this with { DeferredVBType = vbType };

    public ImmutableHashSet<VBType> CandidateTypes { get; init; } = [];
    public IVBInferableType WithCandidateType(VBType vbType) => this with { CandidateTypes = [.. CandidateTypes, vbType] };
}

internal record class VBDeferredTypeMember : TypedSymbol, IVBDeferrableTypeMember
{
    public VBDeferredTypeMember(Uri workspaceRoot, string name, SymbolKindExt kind, Uri parentUri)
        : base(workspaceRoot, name, kind, Accessibility.Public, parentUri, ScopeKind.Module)
    {
    }

    public ImmutableHashSet<VBType> CandidateTypes { get; init; } = [];
    public IVBInferableType WithCandidateType(VBType vbType) => this with { CandidateTypes = [.. CandidateTypes, vbType] };

    public VBType? DeferredVBType { get; init; }
    public VBDeferredTypeMember WithDeferredVBType(VBType vbType) => this with { DeferredVBType = vbType, CandidateTypes = [vbType] };
}

internal record class VBDeferredModuleType : VBDeferredType
{
    public VBDeferredModuleType(string name, Uri uri) : base(name, uri) { }

    public override VBTypedValue DefaultValue => VBVoidValue.Void;
}

internal record class VBDeferredClassModuleType : VBDeferredType
{
    public VBDeferredClassModuleType(string name, Uri uri) : base(name, uri) { }

    public override VBTypedValue DefaultValue => VBObjectValue.Nothing;
}

