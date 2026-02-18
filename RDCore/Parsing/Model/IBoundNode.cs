using RDCore.Parsing.Model.Types;
using RDCore.Parsing.Model.Values;

namespace RDCore.Parsing.Model;

internal interface IBoundNode
{
    VBType ResultType { get; init; }
}

internal interface IExecutableNode
{
    VBTypedValue ResultValue { get; init; }
}