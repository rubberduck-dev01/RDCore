using RDCore.Parsing.Model.Types;
using RDCore.Parsing.Model.Values;
using RDCore.Runtime;
using RDCore.Runtime.Model.Operators;
using RDCore.Server;

namespace RDCore.Parsing.Model.Expressions.Operators;

internal static class SymbolOperation
{
    internal delegate VBTypedValue UnaryOperation(
        VBExecutionContext context,
        VBUnaryOperatorExpression operation,
        VBTypedValue value);

    internal delegate VBTypedValue BinaryOperation(
        VBExecutionContext context,
        VBBinaryOperatorExpression operation,
        VBTypedValue lhsValue,
        VBTypedValue rhsValue);

    private static readonly Dictionary<Uri, BinaryOperation> _binaryInstructions = new()
    {
        [GlobalSymbols.Addition.Uri] = EvaluateBinaryAddition,
        [GlobalSymbols.Subtraction.Uri] = EvaluateBinarySubtraction,
        [GlobalSymbols.Multiplication.Uri] = EvaluateBinaryMultiplication,
        [GlobalSymbols.Division.Uri] = EvaluateBinaryDivision,
        [GlobalSymbols.IntegerDivision.Uri] = EvaluateBinaryIntegerDivision,
        [GlobalSymbols.Exponentiation.Uri] = EvaluateBinaryExponentiation,
        [GlobalSymbols.Modulo.Uri] = EvaluateBinaryModulo,
    };

    private static readonly Dictionary<Uri, UnaryOperation> _unaryInstructions = new()
    {
        [GlobalSymbols.ParenthesizedExp.Uri] = EvaluateUnaryParentheses,
        [GlobalSymbols.UnaryPlus.Uri] = EvaluateUnaryPlus,
        [GlobalSymbols.UnaryMinus.Uri] = EvaluateUnaryMinus,
        [GlobalSymbols.Not.Uri] = EvaluateUnaryBitwiseNot,
    };

    public static BinaryOperation GetBinaryInstruction(Uri uri) => _binaryInstructions[uri];

    private static VBTypedValue EvaluateUnaryOp(VBExecutionContext context,
        VBUnaryOperatorExpression expression,
        VBTypedValue value,
        Func<VBTypedValue, VBTypedValue> op)
    {
        // MS-VBAL 5.6.3: Null Propagation
        if (value is VBNullValue)
        {
            return VBNullValue.Null;
        }

        // MS-VBAL 5.4.3.8: Let-Coercion
        var effectiveValue = value is VBObjectValue obj ? obj.LetCoerce() : value;

        // MS-VBAL 5.6.7 Empty Coercion
        if (effectiveValue is VBEmptyValue)
        {
            effectiveValue = VBIntegerValue.Zero;
        }

        if (effectiveValue is not VBNumericTypedValue)
        {
            throw VBRuntimeErrorException.TypeMismatch(expression.Symbol?.SelectionRange!);
        }

        // MS-VBAL 5.6.7: Unary + and - on Empty results in 0 (Integer)
        // Note: Parentheses on Empty stays Empty until an operator touches it.
        return op(effectiveValue);
    }

    private static VBTypedValue EvaluateNumericBinaryOp(VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs,
        Func<double, double, double> op,
        out VBNumericTypedValue lhsNumeric,
        out VBNumericTypedValue rhsNumeric,
        out VBType targetType
        )
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

    public static VBTypedValue EvaluateBinaryAddition(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs
        )
    {
        VBNumericTypedValue lhsNumeric;
        VBNumericTypedValue rhsNumeric;

        if (lhs is VBStringValue && rhs is VBStringValue)
        {
            // if both operands are strings, then this is a concatenation, not an addition.
            // RDC11006 is issued for the ambiguous concatenation operator usage.
            context.AddDiagnostic(RDCoreDiagnostic.AmbiguousConcatenation(expression.Symbol.Range!));
        }

        var resultValue = EvaluateNumericBinaryOp(context, expression, lhs, rhs,
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

    public static VBTypedValue EvaluateBinarySubtraction(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs
        )
    {
        var resultValue = EvaluateNumericBinaryOp(context, expression, lhs, rhs,
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

    public static VBTypedValue EvaluateBinaryMultiplication(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs
        )
    {
        // MS-VBAL 5.6.9.2
        if (lhs.TypeInfo is VBDateType || rhs.TypeInfo is VBDateType)
        {
            throw VBRuntimeErrorException.TypeMismatch(expression.Symbol?.Range!, "The multiplication operator does not accept Date parameters.");
        }

        return EvaluateNumericBinaryOp(context, expression, lhs, rhs,
            (left, right) => left * right,
            out var lhsNumeric,
            out var rhsNumeric,
            out var targetType);
    }

    public static VBTypedValue EvaluateBinaryExponentiation(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs
        )
    {
        // MS-VBAL 5.6.9.2
        if (lhs.TypeInfo is VBDateType || rhs.TypeInfo is VBDateType)
        {
            throw VBRuntimeErrorException.TypeMismatch(expression.Symbol?.Range!, "The multiplication operator does not accept Date parameters.");
        }

        return EvaluateNumericBinaryOp(context, expression, lhs, rhs,
            (left, right) => Math.Pow(left, right),
            out var lhsNumeric,
            out var rhsNumeric,
            out var targetType);
    }

    public static VBTypedValue EvaluateBinaryDivision(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs
        )
    {
        // MS-VBAL 5.6.9.2
        if (lhs.TypeInfo is VBDateType || rhs.TypeInfo is VBDateType)
        {
            throw VBRuntimeErrorException.TypeMismatch(expression.Symbol?.Range!, "The division operator does not accept Date parameters.");
        }

        return EvaluateNumericBinaryOp(context, expression, lhs, rhs,
            (left, right) => left / right,
            out var lhsNumeric,
            out var rhsNumeric,
            out var targetType);
    }

    public static VBTypedValue EvaluateBinaryIntegerDivision(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs
        )
    {
        // MS-VBAL 5.6.9.2
        if (lhs.TypeInfo is VBDateType || rhs.TypeInfo is VBDateType)
        {
            throw VBRuntimeErrorException.TypeMismatch(expression.Symbol?.Range!, "The integer division operator does not accept Date parameters.");
        }

        return EvaluateNumericBinaryOp(context, expression, lhs, rhs,
            (left, right) => (int)Math.Round(left, 0, MidpointRounding.ToEven) / (int)Math.Round(right, 0, MidpointRounding.ToEven),
            out var lhsNumeric,
            out var rhsNumeric,
            out var targetType);
    }

    public static VBTypedValue EvaluateBinaryModulo(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs
        )
    {
        var result = EvaluateNumericBinaryOp(context, expression, lhs, rhs,
            (left, right) => Math.DivRem((int)left, (int)right).Remainder,
            out var lhsNumeric,
            out var rhsNumeric,
            out var targetType);

        if (lhs.TypeInfo is VBDateType || rhs.TypeInfo is VBDateType)
        {
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitDateSerialConversion(expression.Symbol?.Range!));
        }

        return result;
    }

    public static VBTypedValue EvaluateBinaryIsRefEquality(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs
        )
    {
        if (lhs is not VBObjectValue lObj || rhs is not VBObjectValue rObj)
        {
            throw VBRuntimeErrorException.TypeMismatch(expression.Location.Range);
        }

        var result = lObj.RawAddress == rObj.RawAddress;
        return new VBBooleanValue(expression.Symbol).WithValue(result);
    }

    public static VBTypedValue EvaluateUnaryParentheses(
        VBExecutionContext context,
        VBUnaryOperatorExpression expression,
        VBTypedValue value) =>
    EvaluateUnaryOp(context, expression, value, value => value);

    public static VBTypedValue EvaluateUnaryPlus(
        VBExecutionContext context,
        VBUnaryOperatorExpression expression,
        VBTypedValue value) =>
    EvaluateUnaryOp(context, expression, value, value => value);

    public static VBTypedValue EvaluateUnaryMinus(
        VBExecutionContext context,
        VBUnaryOperatorExpression expression,
        VBTypedValue value) =>
    EvaluateUnaryOp(context, expression, value, value => value);

    public static VBTypedValue EvaluateUnaryBitwiseNot(
        VBExecutionContext context,
        VBUnaryOperatorExpression expression,
        VBTypedValue value) =>
    EvaluateUnaryOp(context, expression, value, v =>
    {
        var num = ((VBNumericTypedValue)v).AsCoercedNumeric();
        return num.WithValue(~(long)num.NumericValue);
    });

    public static VBTypedValue EvaluateBinaryBitwiseEqv(
        VBExecutionContext context,
        VBBinaryOperatorExpression expression,
        VBTypedValue lhs,
        VBTypedValue rhs)
    {
        return EvaluateNumericBinaryOp(context, expression, lhs, rhs, (left, right) => ~((long)left ^ (long)right),
            out var lhsNumeric,
            out var rhsNumeric,
            out var targetType);
    }

    private static VBType GetPromotedType(VBType lhs, VBType rhs)
    {
        // MS-VBAL 5.6.9.4: Type Promotion Hierarchy
        // Double > Single > Long > Integer > Byte
        if (lhs is VBDoubleType || rhs is VBDoubleType)
        {
            return VBDoubleType.TypeInfo;
        }
        if (lhs is VBSingleType || rhs is VBSingleType)
        {
            return VBSingleType.TypeInfo;
        }
        if (lhs is VBLongType || rhs is VBLongType)
        {
            return VBLongType.TypeInfo;
        }
        if (lhs is VBIntegerType || rhs is VBIntegerType)
        {
            return VBIntegerType.TypeInfo;
        }

        return VBByteType.TypeInfo;
    }
}