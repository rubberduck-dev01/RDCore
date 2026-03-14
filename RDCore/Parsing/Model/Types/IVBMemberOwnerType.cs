using RDCore.Parsing.Model.Types.Complex;
using System.Collections.Immutable;

namespace RDCore.Parsing.Model.Types;

internal interface IVBMemberOwnerType
{
    ImmutableArray<VBTypeMemberSymbol> Members { get; init; }
    IVBMemberOwnerType WithMembers(IEnumerable<VBTypeMemberSymbol> members);
}
