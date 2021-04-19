using Gum.IR0;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0.Runtime
{
    public partial class ExpEvaluator
    {
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

        async ValueTask Operator_Multiply_Int_Int_Int(Exp operandExp0, Exp operandExp1, IntValue result, EvalContext context)
        {
            var operandValue0 = evaluator.AllocValue<IntValue>(Type.Int, context);
            var operandValue1 = evaluator.AllocValue<IntValue>(Type.Int, context);

            await evaluator.EvalExpAsync(operandExp0, operandValue0, context);
            await evaluator.EvalExpAsync(operandExp1, operandValue1, context);

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

        void Operator_Int_Int(Exp operand0, Exp operand1, Value result, EvalContext context)
        {
            var operandValue0 = evaluator.AllocValue<IntValue>(Type.Int, context);
            var operandValue1 = evaluator.AllocValue<IntValue>(Type.Int, context);


        }

        async ValueTask EvalCallInternalBinaryOperatorExpAsync(CallInternalBinaryOperatorExp exp, Value result, EvalContext context)
        {
            // operand들을 복사하지 않고 직접 다룬다
            // var operand0 = await evaluator.EvalLocAsync(exp.Operand0, context);
            // var operand1 = await evaluator.EvalLocAsync(exp.Operand1, context);

            switch (exp.Operator)
            {
                case InternalBinaryOperator.Multiply_Int_Int_Int: Operator_Multiply_Int_Int_Int(exp.Operand0, exp.Operand1, result);                        

                case InternalBinaryOperator.Divide_Int_Int_Int:
                    {
                        var operand0 = evaluator.AllocValue<IntValue>(Type.Int, context);
                        var operand1 = evaluator.AllocValue<IntValue>(Type.Int, context);
                        await evaluator.EvalExpAsync(exp.Operand0, operand0, context);
                        await evaluator.EvalExpAsync(exp.Operand1, operand1, context);
                        Operator_Divide_Int_Int_Int(operand0, operand1, (IntValue)result); break;
                    }

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
                default: throw new UnreachableCodeException();
            }
        }

        async ValueTask EvalCallInternalUnaryOperatorExpAsync(CallInternalUnaryOperatorExp exp, Value result, EvalContext context)
        {
            var operand = await evaluator.EvalLocAsync(exp.Operand, context);

            switch (exp.Operator)
            {
                case InternalUnaryOperator.LogicalNot_Bool_Bool: Operator_LogicalNot_Bool_Bool((BoolValue)operand, (BoolValue)result); break;
                case InternalUnaryOperator.UnaryMinus_Int_Int: Operator_UnaryMinus_Int_Int((IntValue)operand, (IntValue)result); break;

                case InternalUnaryOperator.ToString_Bool_String: Operator_ToString_Bool_String((BoolValue)operand, (StringValue)result); break;
                case InternalUnaryOperator.ToString_Int_String: Operator_ToString_Int_String((IntValue)operand, (StringValue)result); break;
                
                default: throw new UnreachableCodeException();
            }
        }

        async ValueTask EvalCallInternalUnaryAssignOperatorExpAsync(CallInternalUnaryAssignOperator exp, Value result, EvalContext context)
        {
            var operand = await evaluator.EvalLocAsync(exp.Operand, context);

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
