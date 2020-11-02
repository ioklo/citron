using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0.Runtime
{
    public partial class ExpEvaluator
    {
        ValueTask<TValue> EvalExpAsync<TValue>(ExpInfo expInfo, EvalContext context)
            where TValue : Value
            => EvalExpAsync<TValue>(expInfo.Exp, expInfo.Type, context);

        async ValueTask<TValue> EvalExpAsync<TValue>(Exp exp, Type type, EvalContext context)
            where TValue : Value
        {
            var operand0 = evaluator.AllocValue(type, context);
            await EvalAsync(exp, operand0, context);
            return (TValue)operand0;
        }

        void Operator_LogicalNot_Bool_Bool(BoolValue operand, BoolValue result)
        {
            result.SetBool(!operand.GetBool());
        }

        // y = ++x;
        // x.Inc(); y = x;
        // y = x++;
        // y = x; x.Inc();

        void Operator_Inc_Int_Void(IntValue operand)
        {
            operand.SetInt(operand.GetInt() + 1);
        }

        void Operator_Dec_Int_Void(IntValue operand)
        {
            operand.SetInt(operand.GetInt() - 1);
        }

        void Operator_UnaryMinus_Int_Int(IntValue operand, IntValue result)
        {
            result.SetInt(-operand.GetInt());
        }

        void Operator_ToString_Bool_String(BoolValue operand, StringValue result)
        {
            result.SetString(operand.GetBool().ToString());
        }

        void Operator_ToString_Int_String(IntValue operand, StringValue result)
        {
            result.SetString(operand.GetInt().ToString());
        }

        void Operator_Multiply_Int_Int_Int(IntValue operand0, IntValue operand1, IntValue result)
        {
            result.SetInt(operand0.GetInt() * operand1.GetInt());
        }

        void Operator_Divide_Int_Int_Int(IntValue operand0, IntValue operand1, IntValue result)
        {
            result.SetInt(operand0.GetInt() / operand1.GetInt());
        }

        void Operator_Modulo_Int_Int_Int(IntValue operand0, IntValue operand1, IntValue result)
        {
            result.SetInt(operand0.GetInt() % operand1.GetInt());
        }

        void Operator_Add_Int_Int_Int(IntValue operand0, IntValue operand1, IntValue result)
        {
            result.SetInt(operand0.GetInt() + operand1.GetInt());
        }

        void Operator_Add_String_String_String(StringValue operand0, StringValue operand1, StringValue result)
        {
            result.SetString(operand0.GetString() + operand1.GetString());
        }

        void Operator_Subtract_Int_Int_Int(IntValue operand0, IntValue operand1, IntValue result)
        {
            result.SetInt(operand0.GetInt() - operand1.GetInt());
        }

        void Operator_LessThan_Int_Int_Bool(IntValue operand0, IntValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetInt() < operand1.GetInt());
        }

        void Operator_LessThan_String_String_Bool(StringValue operand0, StringValue operand1, BoolValue result)
        {   
            result.SetBool(operand0.GetString().CompareTo(operand1.GetString()) < 0);
        }

        void Operator_GreaterThan_Int_Int_Bool(IntValue operand0, IntValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetInt() > operand1.GetInt());
        }

        void Operator_GreaterThan_String_String_Bool(StringValue operand0, StringValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetString().CompareTo(operand1.GetString()) > 0);
        }

        void Operator_LessThanOrEqual_Int_Int_Bool(IntValue operand0, IntValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetInt() <= operand1.GetInt());
        }

        void Operator_LessThanOrEqual_String_String_Bool(StringValue operand0, StringValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetString().CompareTo(operand1.GetString()) <= 0);
        }

        void Operator_GreaterThanOrEqual_Int_Int_Bool(IntValue operand0, IntValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetInt() >= operand1.GetInt());
        }

        void Operator_GreaterThanOrEqual_String_String_Bool(StringValue operand0, StringValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetString().CompareTo(operand1.GetString()) >= 0);
        }

        void Operator_Equal_Int_Int_Bool(IntValue operand0, IntValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetInt() == operand1.GetInt());
        }

        void Operator_Equal_Bool_Bool_Bool(BoolValue operand0, BoolValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetBool() == operand1.GetBool());
        }

        void Operator_Equal_String_String_Bool(StringValue operand0, StringValue operand1, BoolValue result)
        {
            result.SetBool(operand0.GetString().CompareTo(operand1.GetString()) == 0);
        }

        async ValueTask EvalCallInternalBinaryOperatorExpAsync(CallInternalBinaryOperatorExp exp, Value result, EvalContext context)
        {   
            var operand0 = evaluator.AllocValue(exp.Operand0.Type, context);
            await EvalAsync(exp.Operand0.Exp, operand0, context);

            var operand1 = evaluator.AllocValue(exp.Operand1.Type, context);
            await EvalAsync(exp.Operand1.Exp, operand1, context);

            switch (exp.Operator)
            {
                case InternalBinaryOperator.Multiply_Int_Int_Int: Operator_Multiply_Int_Int_Int((IntValue)operand0, (IntValue)operand1, (IntValue)result); break;
                case InternalBinaryOperator.Divide_Int_Int_Int: Operator_Divide_Int_Int_Int((IntValue)operand0, (IntValue)operand1, (IntValue)result); break;
                case InternalBinaryOperator.Modulo_Int_Int_Int: Operator_Modulo_Int_Int_Int((IntValue)operand0, (IntValue)operand1, (IntValue)result); break;
                case InternalBinaryOperator.Add_Int_Int_Int: Operator_Add_Int_Int_Int((IntValue)operand0, (IntValue)operand1, (IntValue)result); break;
                case InternalBinaryOperator.Add_String_String_String: Operator_Add_String_String_String((StringValue)operand0, (StringValue)operand1, (StringValue)result); break;
                case InternalBinaryOperator.Subtract_Int_Int_Int: Operator_Subtract_Int_Int_Int((IntValue)operand0, (IntValue)operand1, (IntValue)result); break;
                case InternalBinaryOperator.LessThan_Int_Int_Bool: Operator_LessThan_Int_Int_Bool((IntValue)operand0, (IntValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.LessThan_String_String_Bool: Operator_LessThan_String_String_Bool((StringValue)operand0, (StringValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.GreaterThan_Int_Int_Bool: Operator_GreaterThan_Int_Int_Bool((IntValue)operand0, (IntValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.GreaterThan_String_String_Bool: Operator_GreaterThan_String_String_Bool((StringValue)operand0, (StringValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool: Operator_LessThanOrEqual_Int_Int_Bool((IntValue)operand0, (IntValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.LessThanOrEqual_String_String_Bool: Operator_LessThanOrEqual_String_String_Bool((StringValue)operand0, (StringValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool: Operator_GreaterThanOrEqual_Int_Int_Bool((IntValue)operand0, (IntValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool: Operator_GreaterThanOrEqual_String_String_Bool((StringValue)operand0, (StringValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.Equal_Int_Int_Bool: Operator_Equal_Int_Int_Bool((IntValue)operand0, (IntValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.Equal_Bool_Bool_Bool: Operator_Equal_Bool_Bool_Bool((BoolValue)operand0, (BoolValue)operand1, (BoolValue)result); break;
                case InternalBinaryOperator.Equal_String_String_Bool: Operator_Equal_String_String_Bool((StringValue)operand0, (StringValue)operand1, (BoolValue)result); break;
                default: throw new InvalidOperationException();
            }
        }

        async ValueTask EvalCallInternalUnaryOperatorExpAsync(CallInternalUnaryOperatorExp exp, Value result, EvalContext context)
        {
            var operand = evaluator.AllocValue(exp.Operand.Type, context);
            await EvalAsync(exp.Operand.Exp, operand, context);

            switch (exp.Operator)
            {
                case InternalUnaryOperator.LogicalNot_Bool_Bool: Operator_LogicalNot_Bool_Bool((BoolValue)operand, (BoolValue)result); break;
                case InternalUnaryOperator.UnaryMinus_Int_Int: Operator_UnaryMinus_Int_Int((IntValue)operand, (IntValue)result); break;

                case InternalUnaryOperator.ToString_Bool_String: Operator_ToString_Bool_String((BoolValue)operand, (StringValue)result); break;
                case InternalUnaryOperator.ToString_Int_String: Operator_ToString_Int_String((IntValue)operand, (StringValue)result); break;
                
                default: throw new InvalidOperationException();
            }
        }

        async ValueTask EvalCallInternalUnaryAssignOperatorExpAsync(CallInternalUnaryAssignOperator exp, Value result, EvalContext context)
        {
            var operand = await GetValueAsync(exp.Operand, context);

            switch(exp.Operator)
            {
                case InternalUnaryAssignOperator.PrefixInc_Int_Int:                    
                    Operator_Inc_Int_Void((IntValue)operand);
                    result.SetValue(operand);
                    break;

                case InternalUnaryAssignOperator.PrefixDec_Int_Int:                    
                    Operator_Dec_Int_Void((IntValue)operand);
                    result.SetValue(operand);
                    break;

                case InternalUnaryAssignOperator.PostfixInc_Int_Int:
                    result.SetValue(operand);
                    Operator_Inc_Int_Void((IntValue)operand);
                    break;

                case InternalUnaryAssignOperator.PostfixDec_Int_Int:
                    result.SetValue(operand);
                    Operator_Dec_Int_Void((IntValue)operand);
                    break;
            }
        }
    }
}
