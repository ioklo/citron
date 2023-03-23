using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using R = Citron.IR0;

namespace Citron
{
    partial struct IR0ExpEvaluator
    {
        async ValueTask Operator_LogicalNot_Bool_Bool(R.Exp operandExp, BoolValue result)
        {
            var operandValue = context.AllocValue<BoolValue>(TypeIds.Bool);
            await EvalAsync(operandExp, context, operandValue);

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
            var operandValue = context.AllocValue<IntValue>(TypeIds.Int);
            await EvalAsync(operandExp, context, operandValue);

            result.SetInt(-operandValue.GetInt());
        }

        async ValueTask Operator_ToString_Bool_String(R.Exp operandExp, StringValue result)
        {
            var operandValue = context.AllocValue<BoolValue>(TypeIds.Bool);
            await EvalAsync(operandExp, context, operandValue);

            result.SetString(operandValue.GetBool() ? "true" : "false");
        }

        async ValueTask Operator_ToString_Int_String(R.Exp operandExp, StringValue result)
        {
            var operandValue = context.AllocValue<IntValue>(TypeIds.Int);
            await EvalAsync(operandExp, context, operandValue);

            result.SetString(operandValue.GetInt().ToString());
        }

        async ValueTask Operator_Multiply_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetInt(operandValue0.GetInt() * operandValue1.GetInt());
        }

        async ValueTask Operator_Divide_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetInt(operandValue0.GetInt() / operandValue1.GetInt());
        }

        async ValueTask Operator_Modulo_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetInt(operandValue0.GetInt() % operandValue1.GetInt());
        }

        async ValueTask Operator_Add_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetInt(operandValue0.GetInt() + operandValue1.GetInt());
        }

        async ValueTask Operator_Add_String_String_String(R.Exp operandExp0, R.Exp operandExp1, StringValue result)
        {
            var operandValue0 = context.AllocValue<StringValue>(TypeIds.String);
            var operandValue1 = context.AllocValue<StringValue>(TypeIds.String);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetString(operandValue0.GetString() + operandValue1.GetString());
        }

        async ValueTask Operator_Subtract_Int_Int_Int(R.Exp operandExp0, R.Exp operandExp1, IntValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetInt(operandValue0.GetInt() - operandValue1.GetInt());
        }

        async ValueTask Operator_LessThan_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetInt() < operandValue1.GetInt());
        }

        async ValueTask Operator_LessThan_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<StringValue>(TypeIds.String);
            var operandValue1 = context.AllocValue<StringValue>(TypeIds.String);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) < 0);
        }

        async ValueTask Operator_GreaterThan_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetInt() > operandValue1.GetInt());
        }

        async ValueTask Operator_GreaterThan_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<StringValue>(TypeIds.String);
            var operandValue1 = context.AllocValue<StringValue>(TypeIds.String);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) > 0);
        }

        async ValueTask Operator_LessThanOrEqual_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetInt() <= operandValue1.GetInt());
        }

        async ValueTask Operator_LessThanOrEqual_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<StringValue>(TypeIds.String);
            var operandValue1 = context.AllocValue<StringValue>(TypeIds.String);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) <= 0);
        }

        async ValueTask Operator_GreaterThanOrEqual_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetInt() >= operandValue1.GetInt());
        }

        async ValueTask Operator_GreaterThanOrEqual_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<StringValue>(TypeIds.String);
            var operandValue1 = context.AllocValue<StringValue>(TypeIds.String);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) >= 0);
        }

        async ValueTask Operator_Equal_Int_Int_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<IntValue>(TypeIds.Int);
            var operandValue1 = context.AllocValue<IntValue>(TypeIds.Int);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetInt() == operandValue1.GetInt());
        }

        async ValueTask Operator_Equal_Bool_Bool_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<BoolValue>(TypeIds.Bool);
            var operandValue1 = context.AllocValue<BoolValue>(TypeIds.Bool);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetBool() == operandValue1.GetBool());
        }

        async ValueTask Operator_Equal_String_String_Bool(R.Exp operandExp0, R.Exp operandExp1, BoolValue result)
        {
            var operandValue0 = context.AllocValue<StringValue>(TypeIds.String);
            var operandValue1 = context.AllocValue<StringValue>(TypeIds.String);

            await EvalAsync(operandExp0, context, operandValue0);
            await EvalAsync(operandExp1, context, operandValue1);

            result.SetBool(operandValue0.GetString().CompareTo(operandValue1.GetString()) == 0);
        }
        

        
        
    }

}
