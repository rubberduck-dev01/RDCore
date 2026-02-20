using RDCore.Parsing.Model.Symbols;
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
        VBOperatorExpression operation,
        VBTypedValue lhsValue,
        VBTypedValue rhsValue);

    private static readonly Dictionary<Uri, BinaryOperation> _binaryInstructions = new()
    {
        { GlobalSymbols.Addition.Uri, EvaluateAddition }
    };

    public static VBTypedValue EvaluateAddition(
        VBExecutionContext context,
        VBOperatorExpression operation,
        VBTypedValue lhs,
        VBTypedValue rhs)
    {
        // MS-VBAL 5.6.9.4: "If either operand is a Null, the result is a Null."
        if (lhs is VBNullValue || rhs is VBNullValue) { return VBNullValue.Null; }

        // MS-VBAL 5.6.9.4: "If both operands are Empty, the result is an Integer 0."
        if (lhs is VBEmptyValue && rhs is VBEmptyValue) { return VBIntegerValue.Zero; }

        // MS-VBAL 5.4.1.2: Numeric String Conversions
        // "If one operand is a String and the other is a numeric... 
        //  the String operand is converted to a Double."
        double lNum = ((INumericCoercion)lhs).AsCoercedNumeric()!.NumericValue;
        double rNum = ((INumericCoercion)rhs).AsCoercedNumeric()!.NumericValue;

        // MS-VBAL 5.6.9.4: Addition Operator Promotion Table
        // "The result type is determined by the types of the operands..."
        // Note: Integer + Integer -> Integer (leads to VBR0006 if > 32767)
        var targetType = GetPromotedType(lhs.TypeInfo, rhs.TypeInfo);

        // MS-VBAL 5.6.9.4: Date Math Rule
        // "If one operand is a Date and the other is numeric, the result is a Date."
        if (lhs.TypeInfo is VBDateType || rhs.TypeInfo is VBDateType)
        {
            // Date math is effectively Double math re-wrapped
            return new VBDateValue(operation.Symbol).WithValue(lNum + rNum);
        }

        // MS-VBAL 5.6.9.4: Overflow Check
        // "If the result is too large for the value range of the result type... 
        //  a run-time error is raised." [VBR0006]
        // SURGICAL: Your VBIntegerValue.WithValue() handles this throw.
        return targetType.CreateValue(operation.Symbol).WithValue(lNum + rNum);
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

    //public static VBTypedValue EvaluateUnaryOpResult(VBExecutionContext context, VBUnaryOperatorExpression opSymbol, TypedSymbol symbol, Func<double, double> unaryOp)
    //{
    //    var type = symbol.ResolvedType;
    //    if (type is UnresolvedType)
    //    {
    //        return UnresolvedType.VBType.DefaultValue;
    //    }

    //    if (type is VBVariantType variant)
    //    {
    //        type = context.CurrentScope.GetTypedValue(symbol).TypeInfo ?? variant.Subtype;
    //    }

    //    if (type is VBNullType)
    //    {
    //        context.AddDiagnostic(RDCoreDiagnostic.UnintendedConstantExpression(symbol));
    //        return new VBNullValue(symbol);
    //    }

    //    //if (opSymbol is VBNotOperator && type is VBBooleanType)
    //    //{
    //    //    var value = (VBBooleanValue)context.CurrentScope.GetTypedValue(symbol);
    //    //    return new VBBooleanValue(symbol).WithValue((int)unaryOp.Invoke(value.AsCoercedNumeric().NumericValue));
    //    //}

    //    if (type is VBEmptyType)
    //    {
    //        context.AddDiagnostic(RDCoreDiagnostic.UnintendedConstantExpression(symbol));
    //        return new VBLongValue(symbol).WithValue(0);
    //    }

    //    if (type is INumericType)
    //    {
    //        var value = unaryOp.Invoke(((INumericValue)context.CurrentScope.GetTypedValue(symbol)).AsDouble().Value);
    //        if (opSymbol is VBNotOperator)
    //        {
    //            return new VBLongValue(symbol).WithValue(value);
    //        }

    //        if (type is VBByteType || type is VBIntegerType)
    //        {
    //            if (type is VBByteType)
    //            {
    //                context.AddDiagnostic(RDCoreDiagnostic.ImplicitWideningConversion(symbol));
    //            }
    //            return new VBIntegerValue(symbol) { NumericValue = (short)value };
    //        }
    //        if (type is VBLongType || !context.Is64BitHost && type is VBLongPtrType)
    //        {
    //            return new VBLongValue(symbol) { NumericValue = (int)value };
    //        }
    //        if (type is VBLongLongType || context.Is64BitHost && type is VBLongPtrType)
    //        {
    //            return new VBLongLongValue(symbol) { NumericValue = (long)value };
    //        }
    //        if (type is VBCurrencyType)
    //        {
    //            return new VBCurrencyValue(symbol) { NumericValue = value };
    //        }
    //        if (type is VBDecimalType)
    //        {
    //            return new VBDecimalValue(symbol) { NumericValue = value };
    //        }
    //        if (type is VBSingleType)
    //        {
    //            return new VBSingleValue(symbol) { NumericValue = (float)value };
    //        }
    //        if (type is VBDoubleType)
    //        {
    //            return new VBDoubleValue(symbol) { NumericValue = (double)value };
    //        }

    //        context.AddDiagnostics(VBRuntimeErrorException.TypeMismatch(symbol.Range));
    //        return VBEmptyValue.Empty;
    //    }

    //    if (context.CurrentScope.GetTypedValue(symbol) is INumericCoercion coercible)
    //    {
    //        var value = unaryOp.Invoke(coercible.AsCoercedNumeric()!.Value);
    //        if (type is VBDateType)
    //        {
    //            context.AddDiagnostic(RDCoreDiagnostic.ImplicitDateSerialConversion(symbol));
    //            return VBDateValue.FromSerial(value);
    //        }
    //        return new VBIntegerValue(symbol) { NumericValue = value };
    //    }

    //    if (type is VBObjectType)
    //    {
    //        var value = context.CurrentScope.GetTypedValue(symbol);
    //        if (value is VBNothingValue)
    //        {
    //            context.AddDiagnostics(VBRuntimeErrorException.ObjectVariableNotSet(symbol));
    //            return VBEmptyValue.Empty;
    //        }

    //        context.AddDiagnostics(VBRuntimeErrorException.ObjectDoesntSupportPropertyOrMethod(symbol));
    //        return VBEmptyValue.Empty;
    //    }
    //    else
    //    {
    //        context.AddDiagnostics(VBRuntimeErrorException.TypeMismatch(symbol.Range));
    //        return VBEmptyValue.Empty;
    //    }
    //}

    //public static VBBooleanValue ExecuteCompareOpResult(VBExecutionContext context, TypedSymbol opSymbol, VBTypedValue lhsValue, VBTypedValue rhsValue, Func<string, string, StringComparison, bool> compareOp)
    //{
    //    if (lhsValue is VBStringValue lhsString)
    //    {
    //        if (rhsValue is IStringCoercion coercible)
    //        {
    //            var rhsString = coercible.AsCoercedString()?.Value;
    //            if (rhsValue.TypeInfo != VBErrorType.TypeInfo)
    //            {
    //                context.AddDiagnostic(RDCoreDiagnostic.ImplicitStringCoercion(rhsValue.Symbol!));
    //            }

    //            // TODO handle VBOptionCompare.Database... somehow
    //            var stringComparison = context.CurrentScope.OptionCompare == VBOptionCompare.Binary
    //                ? StringComparison.Ordinal : StringComparison.InvariantCultureIgnoreCase;

    //            var result = compareOp.Invoke(lhsString.Value!, rhsString!, stringComparison);
    //            return new VBBooleanValue(opSymbol) { Value = result };
    //        }
    //    }

    //    throw VBRuntimeErrorException.TypeMismatch(opSymbol.Range, "The data types involved in this comparison operation are not compatible.");
    //}

    //public static VBBooleanValue ExecuteCompareOpResult(VBExecutionContext context, TypedSymbol opSymbol, VBNumericTypedValue lhsValue, VBTypedValue rhsValue, Func<double, double, bool> compareOp)
    //{
    //    var lhsNumeric = lhsValue.AsDouble().Value;
    //    if (lhsValue.TypeInfo == rhsValue.TypeInfo)
    //    {
    //        return new VBBooleanValue(opSymbol) { Value = compareOp.Invoke(lhsNumeric, ((INumericValue)rhsValue).AsDouble().Value) };
    //    }

    //    if (rhsValue is INumericValue rhsNumeric)
    //    {
    //        if (!rhsValue.TypeInfo.ConvertsSafelyToType(rhsValue.TypeInfo))
    //        {
    //            context.AddDiagnostic(RDCoreDiagnostic.ImplicitNarrowingConversion(rhsValue.Symbol?.Range!));
    //        }

    //        return new VBBooleanValue(opSymbol) { Value = compareOp.Invoke(lhsNumeric, rhsNumeric.AsDouble().Value) };
    //    }

    //    if (rhsValue is INumericCoercion coercible)
    //    {
    //        var rhsCoerced = coercible.AsCoercedNumeric()?.Value ?? 0;
    //        context.AddDiagnostic(RDCoreDiagnostic.ImplicitNumericCoercion(rhsValue.Symbol?.Range!));

    //        return new VBBooleanValue(opSymbol) { Value = compareOp.Invoke(lhsNumeric, rhsCoerced) };
    //    }

    //    throw VBRuntimeErrorException.TypeMismatch(opSymbol?.Range!, "The data types involved in this comparison operation are not compatible.");
    //}

    //public static VBTypedValue EvaluateBinaryOpResult(VBExecutionContext context, BinaryOperatorSymbol opSymbol, VBTypedValue lhsValue, VBTypedValue rhsValue, BinaryOperation op)
    //{
    //}

    public static VBTypedValue EvaluateBinaryOpResult(VBExecutionContext context, OperatorSymbol opSymbol, VBTypedValue lhsValue, VBTypedValue rhsValue)
    {
        var lhsType = lhsValue.TypeInfo!;
        if (lhsType is VBVariantType lhsVariant)
        {
            lhsType = lhsVariant.Subtype;
        }

        var rhsType = rhsValue.TypeInfo!;
        if (rhsType is VBVariantType rhsVariant)
        {
            rhsType = rhsVariant.Subtype;
        }

        if (lhsType is VBNullType || rhsType is VBNullType)
        {
            return VBNullValue.Null;
        }

        if (lhsValue is VBStringValue lhsString)
        {
            return EvaluateStringCoercedNumericOp(context, opSymbol, lhsString, rhsValue, opSymbol.Execute);
        }

        if (lhsValue is VBNumericTypedValue lhsNumericValue)
        {
            return EvaluateNumericOp(context, opSymbol, lhsNumericValue, rhsValue, opSymbol.Execute);
        }

        throw VBRuntimeErrorException.TypeMismatch(opSymbol?.Range!, "The data types involved in this binary operation are not compatible.");
    }

    public static VBTypedValue EvaluateBinaryOpResult(VBExecutionContext context, TypedSymbol opSymbol, VBTypedValue lhsValue, VBTypedValue rhsValue, Func<int, int, int> binaryOp)
    {
        var lhsType = lhsValue.TypeInfo!;
        if (lhsType is VBVariantType lhsVariant)
        {
            lhsType = lhsVariant.Subtype;
        }

        var rhsType = rhsValue.TypeInfo!;
        if (rhsType is VBVariantType rhsVariant)
        {
            rhsType = rhsVariant.Subtype;
        }

        if (lhsType is VBNullType || rhsType is VBNullType)
        {
            context.AddDiagnostic(RDCoreDiagnostic.UnintendedConstantExpression(opSymbol?.SelectionRange!));
            return VBNullValue.Null;
        }

        if (opSymbol is BitwiseOperatorSymbol)
        {
            if (lhsType is VBEmptyType)
            {
                context.AddDiagnostic(RDCoreDiagnostic.UnintendedConstantExpression(lhsValue.Symbol?.SelectionRange!));
                lhsValue = VBLongValue.Zero;
            }
            if (rhsType is VBEmptyType)
            {
                context.AddDiagnostic(RDCoreDiagnostic.UnintendedConstantExpression(rhsValue.Symbol?.SelectionRange!));
                rhsValue = VBLongValue.Zero;
            }
        }

        if (lhsValue is VBStringValue lhsString)
        {
            return EvaluateStringCoercedIntegerOp(context, opSymbol, lhsString, rhsValue, (lhs, rhs) => binaryOp(lhs, rhs));
        }

        if (lhsType is VBDateType)
        {
            var lhsSerialDateValue = new VBDoubleValue(lhsValue.Symbol) { NumericValue = ((VBDateValue)lhsValue).SerialValue };
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitDateSerialConversion(lhsValue.Symbol?.SelectionRange!));
            var result = ((VBDoubleValue)EvaluateIntegerOp(context, opSymbol, lhsSerialDateValue, rhsValue, binaryOp)).NumericValue;

            if (opSymbol is AdditionOperatorSymbol /*|| opSymbol is SubtractionOperatorSymbol*/)
            {
                return VBDateValue.FromSerial(result);
            }

            return new VBDoubleValue(opSymbol) { NumericValue = result };
        }

        if (lhsValue is VBNumericTypedValue lhsNumericValue)
        {
            var result = EvaluateIntegerOp(context, opSymbol, lhsNumericValue, rhsValue, binaryOp);
            if (opSymbol is AdditionOperatorSymbol /*|| opSymbol is SubtractionOperatorSymbol || opSymbol is MultiplicationOperatorSymbol*/)
            {
                return result;
            }
            return ((INumericValue)result).AsDouble();
        }

        if (lhsValue is VBBooleanValue lhsBool && rhsValue is VBBooleanValue rhsBool)
        {
            var lhsNumeric = lhsBool.AsCoercedNumeric().AsLong().Value;
            var rhsNumeric = rhsBool.AsCoercedNumeric().AsLong().Value;
            return new VBBooleanValue(opSymbol).WithValue(binaryOp.Invoke(lhsNumeric, rhsNumeric));
        }

        throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, "The data types involved in this binary operation are not compatible.");
    }

    public static VBTypedValue EvaluateBinaryOpResult(VBExecutionContext context, TypedSymbol opSymbol, VBTypedValue lhsValue, VBTypedValue rhsValue, Func<int, int, double> binaryOp)
    {
        var lhsType = lhsValue.TypeInfo!;
        if (lhsType is VBVariantType lhsVariant)
        {
            lhsType = lhsVariant.Subtype;
        }

        var rhsType = rhsValue.TypeInfo!;
        if (rhsType is VBVariantType rhsVariant)
        {
            rhsType = rhsVariant.Subtype;
        }

        if (lhsType is VBNullType || rhsType is VBNullType)
        {
            context.AddDiagnostic(RDCoreDiagnostic.UnintendedConstantExpression(opSymbol?.SelectionRange!));
            return VBNullValue.Null;
        }

        if (lhsValue is VBStringValue lhsString)
        {
            return EvaluateStringCoercedIntegerOp(context, opSymbol, lhsString, rhsValue, (lhs, rhs) => binaryOp(lhs, rhs));
        }

        if (lhsType is VBDateType)
        {
            var lhsNumericValue = new VBDoubleValue(lhsValue.Symbol) { NumericValue = ((VBDateValue)lhsValue).SerialValue };
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitDateSerialConversion(lhsValue.Symbol?.SelectionRange!));
            var result = ((VBDoubleValue)EvaluateIntegerOp(context, opSymbol, lhsNumericValue, rhsValue, (lhs, rhs) => (int)binaryOp(lhs, rhs))).NumericValue;

            if (opSymbol is AdditionOperatorSymbol /*|| opSymbol is SubtractionOperatorSymbol*/)
            {
                return VBDateValue.FromSerial(result);
            }

            return new VBDoubleValue(opSymbol) { NumericValue = result };
        }

        if (lhsType is INumericType)
        {
            var lhsNumericValue = (VBNumericTypedValue)lhsValue;
            var result = EvaluateIntegerOp(context, opSymbol, lhsNumericValue, rhsValue, (lhs, rhs) => (int)binaryOp(lhs, rhs));
            if (opSymbol is AdditionOperatorSymbol /*|| opSymbol is SubtractionOperatorSymbol*/)
            {
                return result;
            }
            return ((INumericValue)result).AsDouble();
        }

        throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, "The data types involved in this binary operation are not compatible.");
    }

    #region TODO refactor
    private static VBTypedValue EvaluateStringCoercedNumericOp(VBExecutionContext context, TypedSymbol opSymbol, VBStringValue lhsString, VBTypedValue rhsValue, Func<double, double, double> binaryOp)
    {
        if (rhsValue is VBStringValue rhsString && opSymbol is AdditionOperatorSymbol)
        {
            context.AddDiagnostic(RDCoreDiagnostic.PreferConcatOperatorForStringConcatenation(opSymbol?.SelectionRange!));
            return lhsString.WithValue(lhsString.Value + rhsString.Value);
        }

        var rhsType = rhsValue.TypeInfo;
        if (rhsValue is VBNumericTypedValue numeric)
        {
            double lhsNumberValue;
            if (lhsString.Value.StartsWith("&H"))
            {
                try
                {
                    lhsNumberValue = Convert.ToInt32(lhsString.Value.Replace("&H", "0x"), 16);
                }
                catch
                {
                    throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, "This expression evaluates to a `Double`; LHS `String` value must have a numeric value. Consider explicitly validating and converting the values first.");
                }
            }
            else if (!double.TryParse(lhsString.Value, out lhsNumberValue) || !double.IsRealNumber(lhsNumberValue))
            {
                throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, "This expression evaluates to a `Double`; LHS `String` value must have a numeric value. Consider explicitly validating and converting the values first.");
            }

            var rhsNumberValue = numeric.NumericValue;
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitStringCoercion(rhsValue.Symbol?.SelectionRange!));

            return new VBDoubleValue { NumericValue = binaryOp.Invoke((int)lhsNumberValue, (int)rhsNumberValue), Symbol = opSymbol };
        }

        if (rhsType is INumericCoercion coercible)
        {
            if (!double.TryParse(lhsString.Value, out var lhsNumberValue))
            {
                throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, "This expression evaluates to a `Double`; LHS `String` value must have a numeric value. Consider explicitly validating and converting the values first.");
            }

            var rhsNumberValue = coercible.AsCoercedNumeric()?.Value ?? 0;
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitStringCoercion(rhsValue.Symbol?.SelectionRange!));

            return new VBDoubleValue { NumericValue = binaryOp.Invoke(lhsNumberValue, rhsNumberValue), Symbol = opSymbol };
        }
        if (rhsType is VBNullType)
        {
            context.AddDiagnostic(RDCoreDiagnostic.UnintendedConstantExpression(opSymbol?.SelectionRange!));
            return VBNullValue.Null;
        }
        if (rhsType is VBEmptyType)
        {
            return lhsString;
        }
        //if (rhsType is VBObjectType && context.CurrentScope.GetTypedValue(rhsValue.Symbol?.SelectionRange!) is null)
        //{
        //    throw VBCompileErrorException.InvalidUseOfObject(rhsValue.Symbol?.SelectionRange!, "Object could not be let-coerced into a `String`.");
        //}

        throw VBRuntimeErrorException.TypeMismatch(rhsValue.Symbol?.SelectionRange!, $"Could not coerce RHS operand ({rhsType.Name}) into a `String`.");
    }

    private static VBTypedValue EvaluateStringCoercedIntegerOp(VBExecutionContext context, TypedSymbol opSymbol, VBStringValue lhsString, VBTypedValue rhsValue, Func<int, int, double> binaryOp)
    {
        if (rhsValue is VBStringValue rhsString && opSymbol is AdditionOperatorSymbol)
        {
            context.AddDiagnostic(RDCoreDiagnostic.PreferConcatOperatorForStringConcatenation(opSymbol?.SelectionRange!));
            return lhsString.WithValue(lhsString.Value + rhsString.Value);
        }

        var rhsType = rhsValue.TypeInfo;
        if (rhsValue is VBNumericTypedValue numeric)
        {
            double lhsNumberValue;
            if (lhsString.Value.StartsWith("&H"))
            {
                try
                {
                    lhsNumberValue = Convert.ToInt32(lhsString.Value.Replace("&H", "0x"), 16);
                }
                catch
                {
                    throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, "This expression evaluates to a `Double`; LHS `String` value must have a numeric value. Consider explicitly validating and converting the values first.");
                }
            }
            else if (!double.TryParse(lhsString.Value, out lhsNumberValue) || !double.IsRealNumber(lhsNumberValue))
            {
                throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, "This expression evaluates to a `Double`; LHS `String` value must have a numeric value. Consider explicitly validating and converting the values first.");
            }

            var rhsNumberValue = numeric.NumericValue;
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitStringCoercion(rhsValue.Symbol?.SelectionRange!));

            return new VBDoubleValue { NumericValue = binaryOp.Invoke((int)lhsNumberValue, (int)rhsNumberValue), Symbol = opSymbol };
        }

        if (rhsType is INumericCoercion coercible)
        {
            if (!double.TryParse(lhsString.Value, out var lhsNumberValue))
            {
                throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, "This expression evaluates to a `Double`; LHS `String` value must have a numeric value. Consider explicitly validating and converting the values first.");
            }

            var rhsNumberValue = coercible.AsCoercedNumeric()?.Value ?? 0;
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitStringCoercion(rhsValue.Symbol?.SelectionRange!));

            return new VBDoubleValue { NumericValue = binaryOp.Invoke((int)lhsNumberValue, (int)rhsNumberValue), Symbol = opSymbol };
        }
        if (rhsType is VBNullType)
        {
            return VBNullValue.Null;
        }
        if (rhsType is VBEmptyType)
        {
            return lhsString;
        }
        //if (rhsType is VBObjectType && context.CurrentScope.GetTypedValue(rhsValue.Symbol?.SelectionRange!) is null)
        //{
        //    throw VBCompileErrorException.InvalidUseOfObject(rhsValue.Symbol?.SelectionRange!, "Object could not be let-coerced into a `String`.");
        //}

        throw VBRuntimeErrorException.TypeMismatch(rhsValue.Symbol?.SelectionRange!, $"Could not coerce RHS operand ({rhsType.Name}) into a `String`.");
    }
    #endregion

    #region TODO refactor
    private static VBTypedValue EvaluateNumericOp(VBExecutionContext context, OperatorSymbol opSymbol, VBNumericTypedValue lhsNumericValue, VBTypedValue rhsValue, BinaryOperation binaryOp)
    {
        var rhsType = rhsValue.TypeInfo!;

        if (rhsType is INumericType)
        {
            var rhsNumericValue = ((VBNumericTypedValue)rhsValue).AsDouble();
            return lhsNumericValue.AsDouble().WithValue(binaryOp.Invoke(context, opSymbol, lhsNumericValue.AsDouble().Value, rhsNumericValue.Value));
        }

        if (rhsType is VBDateType)
        {
            var rhsDateValue = (VBDateValue)rhsValue;
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitDateSerialConversion(rhsValue.Symbol?.SelectionRange!));

            return new VBDoubleValue(opSymbol).WithValue(binaryOp.Invoke(lhsNumericValue.AsDouble().Value, rhsDateValue.SerialValue));
        }

        if (rhsType is INumericCoercion coercible)
        {
            var rhsCoercedValue = coercible.AsCoercedNumeric();
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitNumericCoercion(opSymbol?.SelectionRange!));

            if (lhsNumericValue.Size >= rhsValue.Size)
            {
                return lhsNumericValue.AsDouble().WithValue(binaryOp.Invoke(lhsNumericValue.AsDouble().Value, rhsCoercedValue?.Value ?? 0));
            }
            else
            {
                return ((VBNumericTypedValue)rhsValue).AsDouble().WithValue(binaryOp.Invoke(lhsNumericValue.AsDouble().Value, rhsCoercedValue?.Value ?? 0));
            }
        }

        throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, $"Could not coerce RHS operand ({rhsType.Name}) into a numeric value.");
    }

    private static VBTypedValue EvaluateIntegerOp(VBExecutionContext context, TypedSymbol opSymbol, VBNumericTypedValue lhsNumericValue, VBTypedValue rhsValue, Func<int, int, int> binaryOp)
    {
        var rhsType = rhsValue.TypeInfo!;

        if (rhsValue is VBNumericTypedValue rhsNumericValue)
        {
            if (lhsNumericValue.Size >= rhsNumericValue.Size)
            {
                return lhsNumericValue.WithValue(binaryOp.Invoke((int)lhsNumericValue.AsDouble().Value, (int)rhsNumericValue.AsDouble().Value));
            }
            else
            {
                return rhsNumericValue.WithValue(binaryOp.Invoke((int)lhsNumericValue.AsDouble().Value, (int)rhsNumericValue.AsDouble().Value));
            }
        }

        if (rhsType is VBDateType)
        {
            var rhsDateValue = (VBDateValue)rhsValue;
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitDateSerialConversion(rhsValue.Symbol?.SelectionRange!));

            return rhsDateValue.WithValue(binaryOp.Invoke((int)rhsDateValue.SerialValue, (int)lhsNumericValue.AsDouble().Value));
        }

        if (rhsValue is INumericCoercion coercible)
        {
            var rhsCoercedValue = coercible.AsCoercedNumeric();
            context.AddDiagnostic(RDCoreDiagnostic.ImplicitNumericCoercion(opSymbol?.SelectionRange!));

            if (lhsNumericValue.Size >= rhsValue.Size)
            {
                return lhsNumericValue.WithValue(binaryOp.Invoke((int)lhsNumericValue.AsDouble().Value, (int)(rhsCoercedValue?.Value ?? 0)));
            }
            else
            {
                return rhsCoercedValue!.AsDouble().WithValue(binaryOp.Invoke((int)lhsNumericValue.AsDouble().Value, (int)(rhsCoercedValue?.Value ?? 0)));
            }
        }

        throw VBRuntimeErrorException.TypeMismatch(opSymbol?.SelectionRange!, $"Could not coerce RHS operand ({rhsType.Name}) into a numeric value.");
    }
    #endregion
}