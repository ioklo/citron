using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        partial class ExpEvaluator
        {
            async ValueTask Operator_LogicalNot_Bool_Bool(R.Exp operandExp, BoolValue result)
            {
                var operandValue = evaluator.AllocValue<BoolValue>(R.Path.Bool);
                await evaluator.EvalExpAsync(operandExp, operandValue);

                result.SetBool(!operandValue.GetBool());
            }

            // y = ++x;
            // x.Inc(); y = x;
            // y = x++;
            // y = x; x.Inc();

            void Operator_Inc_Int_Void(IntValue operandValue)
            {
                operandValue.SetInt(operandValue.GetInt() + 1);
            }

            void Operator_Dec_Int_Void(IntValue operandValue)
            {
                operandValue.SetInt(operandValue.GetInt() - 1);
            }

            async ValueTask Operator_UnaryMinus_Int_Int(R.Exp operandExp, IntValue result)
            {
                var operandValue = evaluator.AllocValue<IntValue>(R.Path.Int);
                await evaluator.EvalExpAsync(operandExp, operandValue);

                result.SetInt(-operandValue.GetInt());
            }

            async ValueTask Operator_ToString_Bool_String(R.Exp operandExp, StringValue result)
            {
                var operandValue = evaluator.AllocValue<BoolValue>(R.Path.Bool);
                await evaluator.EvalExpAsync(operandExp, operandValue);

                result.SetString(operandValue.GetBool().ToString());
            }

            async ValueTask Operator_ToString_Int_String(R.Exp operandExp, StringValue result)
            {
                var operandValue = evaluator.AllocValue<IntValue>(R.Path.Int);
                await evaluator.EvalExpAsync(operandExp, operandValue);

                result.SetString(operandValue.GetInt().ToString());
            }

            async ValueTask Operator_Multiply_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetInt(operandValue0.GetInt() * operandValue1.GetInt());
            }

            async ValueTask Operator_Divide_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetInt(operandValue0.GetInt() / operandValue1.GetInt());
            }

            async ValueTask Operator_Modulo_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetInt(operandValue0.GetInt() % operandValue1.GetInt());
            }

            async ValueTask Operator_Add_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetInt(operandValue0.GetInt() + operandValue1.GetInt());
            }

            async ValueTask Operator_Add_String_String_String(R.Exp operandExp0, R.Exp operandExp1, StringValue result)
            {
                var operandValue0 = evaluator.AllocValue<StringValue>(R.Path.String);
                var operandValue1 = evaluator.AllocValue<StringValue>(R.Path.String);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetString(operandValue0.GetString() + operandValue1.GetString());
            }

            async ValueTask Operator_Subtract_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetInt(operandValue0.GetInt() - operandValue1.GetInt());
            }

            async ValueTask Operator_LessThan_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetInt() < operandValue1.GetInt());
            }

            async ValueTask Operator_LessThan_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<StringValue>(R.Path.String);
                var operandValue1 = evaluator.AllocValue<StringValue>(R.Path.String);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) < 0);
            }

            async ValueTask Operator_GreaterThan_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetInt() > operandValue1.GetInt());
            }

            async ValueTask Operator_GreaterThan_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<StringValue>(R.Path.String);
                var operandValue1 = evaluator.AllocValue<StringValue>(R.Path.String);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) > 0);
            }

            async ValueTask Operator_LessThanOrEqual_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetInt() <= operandValue1.GetInt());
            }

            async ValueTask Operator_LessThanOrEqual_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<StringValue>(R.Path.String);
                var operandValue1 = evaluator.AllocValue<StringValue>(R.Path.String);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) <= 0);
            }

            async ValueTask Operator_GreaterThanOrEqual_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetInt() >= operandValue1.GetInt());
            }

            async ValueTask Operator_GreaterThanOrEqual_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<StringValue>(R.Path.String);
                var operandValue1 = evaluator.AllocValue<StringValue>(R.Path.String);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) >= 0);
            }

            async ValueTask Operator_Equal_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<IntValue>(R.Path.Int);
                var operandValue1 = evaluator.AllocValue<IntValue>(R.Path.Int);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetInt() == operandValue1.GetInt());
            }

            async ValueTask Operator_Equal_Bool_Bool_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<BoolValue>(R.Path.Bool);
                var operandValue1 = evaluator.AllocValue<BoolValue>(R.Path.Bool);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetBool() == operandValue1.GetBool());
            }

            async ValueTask Operator_Equal_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
            {
                var operandValue0 = evaluator.AllocValue<StringValue>(R.Path.String);
                var operandValue1 = evaluator.AllocValue<StringValue>(R.Path.String);

                await evaluator.EvalExpAsync(operandExp0, operandValue0);
                await evaluator.EvalExpAsync(operandExp1, operandValue1);

                result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) == 0);
            }

            ValueTask EvalCallInternalBinaryOperatorExpAsync(R.CallInternalBinaryOperatorExp exp, Value result)
            {
                switch (exp.Operator)
                {
                    case R.InternalBinaryOperator.Multiply_Int_Int_Int: return Operator_Multiply_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                    case R.InternalBinaryOperator.Divide_Int_Int_Int: return Operator_Divide_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                    case R.InternalBinaryOperator.Modulo_Int_Int_Int: return Operator_Modulo_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                    case R.InternalBinaryOperator.Add_Int_Int_Int: return Operator_Add_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                    case R.InternalBinaryOperator.Add_String_String_String: return Operator_Add_String_String_String(exp.Operand0, exp.Operand1, (StringValue)result);
                    case R.InternalBinaryOperator.Subtract_Int_Int_Int: return Operator_Subtract_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                    case R.InternalBinaryOperator.LessThan_Int_Int_Bool: return Operator_LessThan_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.LessThan_String_String_Bool: return Operator_LessThan_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.GreaterThan_Int_Int_Bool: return Operator_GreaterThan_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.GreaterThan_String_String_Bool: return Operator_GreaterThan_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool: return Operator_LessThanOrEqual_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool: return Operator_LessThanOrEqual_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool: return Operator_GreaterThanOrEqual_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool: return Operator_GreaterThanOrEqual_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.Equal_Int_Int_Bool: return Operator_Equal_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.Equal_Bool_Bool_Bool: return Operator_Equal_Bool_Bool_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    case R.InternalBinaryOperator.Equal_String_String_Bool: return Operator_Equal_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                    default: throw new UnreachableCodeException();
                }
            }

            ValueTask EvalCallInternalUnaryOperatorExpAsync(R.CallInternalUnaryOperatorExp exp, Value result)
            {
                switch (exp.Operator)
                {
                    case R.InternalUnaryOperator.LogicalNot_Bool_Bool: return Operator_LogicalNot_Bool_Bool(exp.Operand, (BoolValue)result);
                    case R.InternalUnaryOperator.UnaryMinus_Int_Int: return Operator_UnaryMinus_Int_Int(exp.Operand, (IntValue)result);

                    case R.InternalUnaryOperator.ToString_Bool_String: return Operator_ToString_Bool_String(exp.Operand, (StringValue)result);
                    case R.InternalUnaryOperator.ToString_Int_String: return Operator_ToString_Int_String(exp.Operand, (StringValue)result);

                    default: throw new UnreachableCodeException();
                }
            }

            // ++x
            async ValueTask EvalCallInternalUnaryAssignOperatorExpAsync(R.CallInternalUnaryAssignOperator exp, Value result)
            {
                var operand = await evaluator.EvalLocAsync(exp.Operand);

                switch (exp.Operator)
                {
                    case R.InternalUnaryAssignOperator.PrefixInc_Int_Int:
                        Operator_Inc_Int_Void((IntValue)operand);
                        result.SetValue(operand);
                        break;

                    case R.InternalUnaryAssignOperator.PrefixDec_Int_Int:
                        Operator_Dec_Int_Void((IntValue)operand);
                        result.SetValue(operand);
                        break;

                    case R.InternalUnaryAssignOperator.PostfixInc_Int_Int:
                        result.SetValue(operand);
                        Operator_Inc_Int_Void((IntValue)operand);
                        break;

                    case R.InternalUnaryAssignOperator.PostfixDec_Int_Int:
                        result.SetValue(operand);
                        Operator_Dec_Int_Void((IntValue)operand);
                        break;
                }
            }
        }
    }
}
