using RDCore.Runtime.Execution.Frames;
using RDCore.Runtime.Semantics.LetCoercion;
using RDCore.SDK.Model.AST.Expressions;
using RDCore.SDK.Model.Types;
using RDCore.SDK.Model.Types.Abstract;
using RDCore.SDK.Model.Values;
using RDCore.SDK.Model.Values.Abstract;
using RDCore.SDK.Model.Values.Intrinsic;
using RDCore.SDK.Runtime.Abstract.Execution;
using RDCore.SDK.Runtime.Shared;
using RDCore.SDK.Semantics;
using RDCore.SDK.Semantics.Context;
using RDCore.SDK.Semantics.Flags;
using RDCore.SDK.Services.VerboseMessages;

namespace RDCore.Runtime.Semantics.Operators.Logical;

/// <summary>
/// MS-VBAL 5.6.9.8.6 Binary 'Imp' Operator
/// </summary>
public record class BinaryImpLogicalOperatorRuntimeSemantics(
    ILetCoercionRuntimeSemanticsProvider LetCoercionSemanticsProvider,
    IVerboseMessageBuilder FormatterService)
    : BinaryLogicalOperatorRuntimeSemantics(LetCoercionSemanticsProvider, FormatterService)
{
    protected override double EvaluateBitwiseOp(int lhs, int rhs) => (~lhs) | rhs;

    /// <summary>
    /// Evaluates the not-bitwise evaluation branches of the MS-VBAL specifications for a logical operator.
    /// </summary>
    /// <remarks>
    /// Operands are <strong>explicitly specified</strong> as being evaluated bitwise only given specific operand data types.
    /// Base implementation has already handled the case where both operands are <see cref="IIntegralNumericType"/>, and the case where they're both <see cref="VBNullValue"/>.
    /// </remarks>
    protected override RuntimeSemanticsEvaluationResult EvaluateSemanticallly(
        IVBExecutionContext context, 
        VBBinaryOperatorExpression<BinaryLogicalOperatorSemanticContext, LogicalOperatorSemanticFlags> expression, 
        OperatorEvaluationFrame frame)
    {
        var lhs = frame[InputIndex.BinaryLeftOperand];
        var rhs = frame[InputIndex.BinaryRightOperand];

        if (lhs.TypeInfo is IIntegralNumericType && rhs.TypeInfo is IIntegralNumericType
            && lhs is VBNumericTypedValue lhsIntegralNumeric && rhs is VBNumericTypedValue rhsIntegralNumeric)
        {
            return RuntimeSemanticsEvaluationResult.Success(VBTypedValueFactory.CreateValue(VBIntegerType.TypeInfo,
                EvaluateBitwiseOp((double)lhsIntegralNumeric.ManagedValue.RuntimeValue!.BoxedValue, 
                                  (double)rhsIntegralNumeric.ManagedValue.RuntimeValue!.BoxedValue)));
        }
        else if (lhs is VBNumericTypedValue lhsNumeric && rhs is VBNullValue)
        {
            return (double)lhsNumeric.ManagedValue.RuntimeValue!.BoxedValue != (double)VBIntegerType.NegativeOne.ManagedValue.RuntimeValue!.BoxedValue
                ? RuntimeSemanticsEvaluationResult.Success(
                    VBTypedValueFactory.CreateValue(VBIntegerType.TypeInfo, 
                    EvaluateBitwiseOp((int)lhsNumeric.ManagedValue.RuntimeValue!.BoxedValue, (int)VBIntegerType.Zero.ManagedValue.RuntimeValue!.BoxedValue)))
                : EvaluateNullBinaryExpressionResult();
        }
        else if (lhs is VBNullValue && rhs.TypeInfo is IIntegralNumericType && rhs is VBNumericTypedValue rhsNumeric && (double)rhsNumeric.ManagedValue.RuntimeValue!.BoxedValue != 0)
        {
            return RuntimeSemanticsEvaluationResult.Success(
                VBTypedValueFactory.CreateValue(frame.EffectiveType, (double)rhsNumeric.ManagedValue.RuntimeValue!.BoxedValue));
        }
        else if (lhs is VBNullValue && rhs is VBNumericTypedValue rhsMaybeZero && (double)rhsMaybeZero.ManagedValue.RuntimeValue!.BoxedValue == 0)
        {
            return EvaluateNullBinaryExpressionResult();
        }
        else if (lhs is VBNullValue && rhs is VBNullValue)
        {
            return EvaluateNullBinaryExpressionResult();
        }

        return RuntimeSemanticsEvaluationResult.InternalError();
    }
}
