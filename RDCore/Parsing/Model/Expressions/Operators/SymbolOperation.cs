using RDCore.Parsing.Model.Types;
using RDCore.Parsing.Model.Values;
using RDCore.Runtime;
using RDCore.Runtime.Model.Operators;
using RDCore.Server;

namespace RDCore.Parsing.Model.Expressions.Operators;

internal static class SymbolOperation
{
    internal delegate VBTypedValue BinaryOperation(
        VBExecutionContext context,
        VBBinaryOperatorExpression operation,
        VBTypedValue lhsValue,
        VBTypedValue rhsValue);

    private static readonly Dictionary<Uri, BinaryOperation> _binaryInstructions = new()
    {
        [GlobalSymbols.Addition.Uri] = EvaluateAddition,
        [GlobalSymbols.Subtraction.Uri] = EvaluateSubtraction,
    };

    private static VBTypedValue EvaluateNumericBinaryOp(VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs,
        Func<double, double, double> op,
        out VBNumericTypedValue lhsNumeric,
        out VBNumericTypedValue rhsNumeric,
        out VBType targetType)
    {
        lhsNumeric = default!;
        rhsNumeric = default!;

        // MS-VBAL 5.6.9.4: If either operand is Null, the result is Null.
        if (lhs is VBNullValue || rhs is VBNullValue)
        {
            targetType = VBNullType.TypeInfo;
            return VBNullValue.Null;
        }

        // MS-VBAL 5.6.9.4: If both operands are Empty, the result is an Integer 0.
        if (lhs is VBEmptyValue && rhs is VBEmptyValue)
        {
            targetType = VBIntegerType.TypeInfo;
            return VBIntegerValue.Zero;
        }

        // MS-VBAL 5.4.1.2: Numeric String Conversions
        // If one operand is a String and the other is a numeric, the String operand is converted to a Double.
        // RDC00109 is issued for each such implicit coercion, twice if both sides require numeric coercion.
        lhsNumeric = lhs.TypeInfo is INumericType ? (VBNumericTypedValue)lhs : ((INumericCoercion)lhs).AsCoercedNumeric()!;
        rhsNumeric = rhs.TypeInfo is INumericType ? (VBNumericTypedValue)rhs : ((INumericCoercion)rhs).AsCoercedNumeric()!;

        if (lhs is VBStringValue)
        {
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitNumericCoercion(expression.Left.Location.Range, lhs.TypeInfo, VBDoubleType.TypeInfo));
        }

        if (rhs is VBStringValue)
        {
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitNumericCoercion(expression.Right.Location.Range, rhs.TypeInfo, VBDoubleType.TypeInfo));
        }

        // MS-VBAL 5.6.9.4: determine the target type
        targetType = GetPromotedType(lhsNumeric.TypeInfo, rhsNumeric.TypeInfo);

        // calculate the numeric result
        var resultValue = op(lhsNumeric.NumericValue, rhsNumeric.NumericValue);

        // MS-VBAL 5.6.9.4: Overflow Check [VBR0006]
        return (VBTypedValue)((VBNumericTypedValue)targetType.CreateValue(expression.Symbol)).WithValue(resultValue);
    }

    public static VBTypedValue EvaluateAddition(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs)
    {
        VBTypedValue resultValue;
        VBNumericTypedValue lhsNumeric;
        VBNumericTypedValue rhsNumeric;

        if (lhs is VBStringValue && rhs is VBStringValue)
        {
            // if both operands are strings, then this is a concatenation, not an addition.
            // RDC11006 is issued for the ambiguous concatenation operator usage.
            context.AddDiagnostic(RDCoreDiagnostic.AmbiguousConcatenation(expression.Symbol.Range!));
        }

        resultValue = EvaluateNumericBinaryOp(context, expression, lhs, rhs,
            (left, right) => left + right,
            out lhsNumeric,
            out rhsNumeric,
            out var targetType);

        // MS-VBAL 5.6.9.4: Date Math Rule
        // "If one operand is a Date and the other is numeric, the result is a Date."
        if (lhs.TypeInfo is VBDateType || rhs.TypeInfo is VBDateType)
        {
            // Date math is effectively Double math re-wrapped
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitDateSerialConversion(expression.Symbol?.Range!));
            return new VBDateValue(expression.Symbol).WithValue(((VBNumericTypedValue)resultValue).NumericValue);
        }

        return resultValue;
    }

    public static VBTypedValue EvaluateSubtraction(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs)
    {
        VBTypedValue resultValue;

        resultValue = EvaluateNumericBinaryOp(context, expression, lhs, rhs,
            (left, right) => left - right,
            out var lhsNumeric,
            out var rhsNumeric,
            out var targetType);

        // MS-VBAL 5.6.9.4: Date Math Rule
        // "If one operand is a Date and the other is numeric, the result is a Date."
        if (lhs.TypeInfo is VBDateType || rhs.TypeInfo is VBDateType)
        {
            // Date math is effectively Double math re-wrapped
            var diff = lhsNumeric.NumericValue - rhsNumeric.NumericValue;
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitDateSerialConversion(expression.Symbol?.Range!));

            // If both are dates, return Double. If only one is a date, return Date.
            return (lhs.TypeInfo is VBDateType && rhs.TypeInfo is VBDateType)
                ? new VBDoubleValue(expression.Symbol).WithValue(diff)
                : new VBDateValue(expression.Symbol).WithValue(diff);
        }

        return resultValue;
    }

    private static VBType GetPromotedType(VBType lhs, VBType rhs)
    {
        // MS-VBAL 5.6.9.4: Type Promotion Hierarchy
        // Double > Single > Long > Integer > Byte
        if (lhs is VBDoubleType || rhs is VBDoubleType) return VBDoubleType.TypeInfo;
        if (lhs is VBSingleType || rhs is VBSingleType) return VBSingleType.TypeInfo;
        if (lhs is VBLongType || rhs is VBLongType) return VBLongType.TypeInfo;

        return VBIntegerType.TypeInfo;
    }
}