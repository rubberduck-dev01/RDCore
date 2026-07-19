using RDCore.SDK.Model.Values.Abstract;
using RDCore.SDK.Runtime.Abstract.Execution;

namespace RDCore.SDK.Model.Values.Bindings;

/// <summary>
/// Represents a handle to an invalid binding.
/// </summary>
/// <remarks>
/// 💥 All methods throw <see cref="NotSupportedException"/>.
/// </remarks>
public record class InvalidBindingHandle : IBindingHandle
{
    public static InvalidBindingHandle Default { get; } = new();

    public BindingCapabilities BindingCapabilities => BindingCapabilities.None;

    public VBTypedValue GetValue(IVBExecutionContext context) => throw new NotSupportedException();

    public VBTypedValue Invoke(IVBExecutionContext context, VBTypedValue[] args) => throw new NotSupportedException();

    public void SetValue(IVBExecutionContext context, VBTypedValue value) => throw new NotSupportedException();
}