using RDCore.Runtime.Semantics.Abstract;
using RDCore.SDK.Model.Symbols.Abstract;
using RDCore.SDK.Model.Values.Abstract;
using RDCore.SDK.Runtime.Abstract.Execution;
using System.Collections.Immutable;

namespace RDCore.Runtime.Execution.Frames;

/// <summary>
/// Represents a <see cref="StatementRuntimeSemantics{TContext, TFlags}"/> evaluation step.
/// </summary>
/// <param name="NodeId">The <c>Identity</c> of the associated expression node.</param>
/// <param name="StatementSymbol">The <see cref="StaticSymbol"/> representing the <strong>unallocated</strong> <em>language-level statement</em> symbol.</param>
/// <param name="Inputs">The resolved <see cref="VBTypedValue"/> values of the inputs of the statement.</param>
public readonly record struct StatementEvaluationFrame<TInputs>(
    Guid NodeId,
    StaticSymbol StatementSymbol,
    ImmutableArray<VBTypedValue> Inputs) : IStackFrame<TInputs>
where TInputs : struct, Enum
{
    /// <summary>
    /// Gets the input at the specified <c>index</c>.
    /// </summary>
    /// <param name="index">The <strong>zero-based index</strong> of the input value to retrieve.</param>
    /// <returns>The <see cref="VBTypedValue"/> value at the specified index.</returns>
    public VBTypedValue this[int index] => Inputs[index];

    Guid IStackFrame.NodeId => NodeId;
    ImmutableArray<VBTypedValue> IStackFrame.Inputs => Inputs;

    VBTypedValue IStackFrame<TInputs>.this[TInputs value] => Inputs[Convert.ToInt32(value)];
}
