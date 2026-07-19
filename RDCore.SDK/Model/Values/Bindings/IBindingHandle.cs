using RDCore.SDK.Model.Values.Abstract;
using RDCore.SDK.Model.Values.Meta;
using RDCore.SDK.Runtime.Abstract.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDCore.SDK.Model.Values.Bindings;

[Flags]
public enum BindingCapabilities
{
    /// <summary>
    /// Signals a binding that supports no valid operations.
    /// </summary>
    None = 0,
    /// <summary>
    /// Signals a binding's capability to yield a <see cref="VBMemberDescValue"/>.
    /// </summary>
    GetMember = 1 << 0,
    /// <summary>
    /// Signals a binding's capability to yield a <see cref="VBTypedValue"/>.
    /// </summary>
    GetValue = 1 << 1,
    /// Signals a binding's capability to accept a <see cref="VBTypedValue"/>.
    SetValue = 1 << 2,
    /// <summary>
    /// Signals a binding's capability to invoke a callable entity.
    /// </summary>
    Invoke = 1 << 3,
    /// <summary>
    /// Signals a binding's capability to yield an indexed <see cref="VBTypedValue"/>.
    /// </summary>
    GetIndex = 1 << 4,
    /// <summary>
    /// Signals a binding's capability to yield an enumerator.
    /// </summary>
    GetEnumerator = 1 << 5
}

/// <summary>
/// A handle to a binding to a runtime entity.
/// </summary>
public interface IBindingHandle
{
    /// <summary>
    /// Gets the value associated to this handle.
    /// </summary>
    /// <remarks>
    /// 👉 Verify that the binding supports <see cref="BindingCapabilities.GetValue"/>.
    /// </remarks>
    /// <exception cref="NotSupportedException"></exception>
    VBTypedValue GetValue(IVBExecutionContext context);
    /// <summary>
    /// Sets the value associated to this handle.
    /// </summary>
    /// <remarks>
    /// 👉 Verify that the binding supports <see cref="BindingCapabilities.SetValue"/>.
    /// </remarks>
    /// <exception cref="NotSupportedException"></exception>
    void SetValue(IVBExecutionContext context, VBTypedValue value);
    /// <summary>
    /// Invokes the callable entity associated to this handle.
    /// </summary>
    /// <remarks>
    /// 👉 Verify that the binding supports <see cref="BindingCapabilities.SetValue"/>.
    /// </remarks>
    /// <exception cref="NotSupportedException"></exception>
    VBTypedValue Invoke(IVBExecutionContext context, VBTypedValue[] args);

    /// <summary>
    /// Indicates the valid members of this binding.
    /// </summary>
    BindingCapabilities BindingCapabilities { get; }
}
