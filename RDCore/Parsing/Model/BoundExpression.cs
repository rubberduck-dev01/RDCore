using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RDCore.Parsing.Model.Types;

namespace RDCore.Parsing.Model;

internal abstract record class BoundExpression(Location Location) : IBoundNode
{
    public Location Location { get; init; } = Location;
    public VBType ResultType { get; init; } = UnresolvedType.VBType;

    public BoundExpression WithResultType(VBType type) => this with { ResultType = type };
}
