using Gum.CompileTime;
using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    public partial class ExpEvaluator
    {
        ValueTask<TValue> EvalExpAndTypeAsync<TValue>(ExpAndType expAndType, EvalContext context)
            where TValue : Value
            => EvalExpAndTypeAsync<TValue>(expAndType.Exp, expAndType.TypeValue, context);

        async ValueTask<TValue> EvalExpAndTypeAsync<TValue>(Exp exp, TypeValue type, EvalContext context)
            where TValue : Value
        {
            var value0 = evaluator.GetDefaultValue(type, context);
            await EvalAsync(exp, value0, context);
            return (TValue)value0;
        }

        void Operator_LogicalNot_Bool_Bool(BoolValue arg0, BoolValue result)
        {
            result.SetBool(!arg0.GetBool());
        }

        // y = ++x;
        // x.Inc(); y = x;
        // y = x++;
        // y = x; x.Inc();

        void Operator_Inc_Int_Void(IntValue arg0)
        {
            arg0.SetInt(arg0.GetInt() + 1);
        }

        void Operator_Dec_Int_Void(IntValue arg0, EvalContext context)
        {
            arg0.SetInt(arg0.GetInt() - 1);
        }

        async ValueTask Operator_UnaryMinus_Int_Int(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            ((IntValue)result).SetInt(-value0.GetInt());
        }

        async ValueTask Operator_Multiply_Int_Int_Int(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((IntValue)result).SetInt(value0.GetInt() * value1.GetInt());
        }

        async ValueTask Operator_Divide_Int_Int_Int(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((IntValue)result).SetInt(value0.GetInt() / value1.GetInt());
        }

        async ValueTask Operator_Modulo_Int_Int_Int(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((IntValue)result).SetInt(value0.GetInt() % value1.GetInt());
        }

        async ValueTask Operator_Add_Int_Int_Int(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((IntValue)result).SetInt(value0.GetInt() + value1.GetInt());
        }

        async ValueTask Operator_Add_String_String_String(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<StringValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<StringValue>(operands[1], context);

            ((StringValue)result).SetString(value0.GetString() + value1.GetString());
        }

        async ValueTask Operator_Substract_Int_Int_Int(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((IntValue)result).SetInt(value0.GetInt() - value1.GetInt());
        }

        async ValueTask Operator_LessThan_Int_Int_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetInt() < value1.GetInt());
        }

        async ValueTask Operator_LessThan_String_String_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {   
            var value0 = await EvalExpAndTypeAsync<StringValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<StringValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetString().CompareTo(value1.GetString()) < 0);
        }

        async ValueTask Operator_GreaterThan_Int_Int_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetInt() > value1.GetInt());
        }

        async ValueTask Operator_GreaterThan_String_String_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<StringValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<StringValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetString().CompareTo(value1.GetString()) > 0);
        }

        async ValueTask Operator_LessThanOrEqual_Int_Int_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetInt() <= value1.GetInt());
        }

        async ValueTask Operator_LessThanOrEqual_String_String_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<StringValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<StringValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetString().CompareTo(value1.GetString()) <= 0);
        }

        async ValueTask Operator_GreaterThanOrEqual_Int_Int_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetInt() >= value1.GetInt());
        }

        async ValueTask Operator_GreaterThanOrEqual_String_String_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<StringValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<StringValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetString().CompareTo(value1.GetString()) >= 0);
        }

        async ValueTask Operator_Equal_Int_Int_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<IntValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<IntValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetInt() == value1.GetInt());
        }

        async ValueTask Operator_Equal_Bool_Bool_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<BoolValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<BoolValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetBool() == value1.GetBool());
        }

        async ValueTask Operator_Equal_String_String_Bool(ImmutableArray<ExpAndType> operands, Value result, EvalContext context)
        {
            var value0 = await EvalExpAndTypeAsync<StringValue>(operands[0], context);
            var value1 = await EvalExpAndTypeAsync<StringValue>(operands[1], context);

            ((BoolValue)result).SetBool(value0.GetString().CompareTo(value1.GetString()) == 0);
        }

        async ValueTask EvalCallInternalOperatorExpAsync(CallInternalOperatorExp exp, Value result, EvalContext context)
        {
            var operandValues = new List<Value>();
            foreach (var operand in exp.Operands)
            {
                var operandValue = evaluator.GetDefaultValue(operand.TypeValue, context);
                operandValues.Add(operandValue);

                await EvalAsync(operand.Exp, operandValue, context);
            }

            switch (exp.Operator)
            {
                case InternalOperator.LogicalNot_Bool_Bool:
                    Operator_LogicalNot_Bool_Bool((BoolValue)operandValues[0], (BoolValue)result);
                    return;

                case InternalOperator.Inc_Int_Void:
                    var arg0 = await EvalExpAndTypeAsync<IntValue>(exp.Operands[0], context);
                    Operator_Inc_Int_Void(arg0, context);
                        return;
                    }

                case InternalOperator.Dec_Int_Void:
                    return Operator_Dec_Int_Void(exp.Operands, result, context);

                case InternalOperator.UnaryMinus_Int_Int:
                    return Operator_UnaryMinus_Int_Int(exp.Operands, result, context);

                case InternalOperator.Multiply_Int_Int_Int:
                    return Operator_Multiply_Int_Int_Int(exp.Operands, result, context);

                case InternalOperator.Divide_Int_Int_Int:
                    return Operator_Divide_Int_Int_Int(exp.Operands, result, context);

                case InternalOperator.Modulo_Int_Int_Int:
                    return Operator_Modulo_Int_Int_Int(exp.Operands, result, context);

                case InternalOperator.Add_Int_Int_Int:
                    return Operator_Add_Int_Int_Int(exp.Operands, result, context);

                case InternalOperator.Add_String_String_String:
                    return Operator_Add_String_String_String(exp.Operands, result, context);

                case InternalOperator.Substract_Int_Int_Int:
                    return Operator_Substract_Int_Int_Int(exp.Operands, result, context);

                case InternalOperator.LessThan_Int_Int_Bool:
                    return Operator_LessThan_Int_Int_Bool(exp.Operands, result, context);

                case InternalOperator.LessThan_String_String_Bool:
                    return Operator_LessThan_String_String_Bool(exp.Operands, result, context);

                case InternalOperator.GreaterThan_Int_Int_Bool:
                    return Operator_GreaterThan_Int_Int_Bool(exp.Operands, result, context);

                case InternalOperator.GreaterThan_String_String_Bool:
                    return Operator_GreaterThan_String_String_Bool(exp.Operands, result, context);

                case InternalOperator.LessThanOrEqual_Int_Int_Bool:
                    return Operator_LessThanOrEqual_Int_Int_Bool(exp.Operands, result, context);

                case InternalOperator.LessThanOrEqual_String_String_Bool:
                    return Operator_LessThanOrEqual_String_String_Bool(exp.Operands, result, context);

                case InternalOperator.GreaterThanOrEqual_Int_Int_Bool:
                    return Operator_GreaterThanOrEqual_Int_Int_Bool(exp.Operands, result, context);

                case InternalOperator.GreaterThanOrEqual_String_String_Bool:
                    return Operator_GreaterThanOrEqual_String_String_Bool(exp.Operands, result, context);

                case InternalOperator.Equal_Int_Int_Bool:
                    return Operator_Equal_Int_Int_Bool(exp.Operands, result, context);

                case InternalOperator.Equal_Bool_Bool_Bool:
                    return Operator_Equal_Bool_Bool_Bool(exp.Operands, result, context);

                case InternalOperator.Equal_String_String_Bool:
                    return Operator_Equal_String_String_Bool(exp.Operands, result, context);

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
