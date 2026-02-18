using RDCore.Parsing.Model.Symbols;

namespace RDCore.Parsing.Model.Types;

internal interface IVBDeclaredType
{
    Symbol Declaration { get; init; }
    Symbol[]? Definitions { get; init; }
}
