using Gum.Runtime;
using Gum.StaticAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.Runtime.Evaluator;
using static Gum.Infra.CollectionExtensions;
using Gum.CompileTime;
using Gum;
using Gum.IR0;
using System.Collections.Immutable;

namespace Gum.Runtime
{
    public partial class ExpEvaluator
    {
        private Evaluator evaluator;

        public ExpEvaluator(Evaluator evaluator)
        {
            this.evaluator = evaluator;
        }
        
        public async ValueTask<Value> GetValueAsync(StorageInfo storageInfo, EvalContext context)
        {
            switch(storageInfo)
            {
                // 모듈
                case StorageInfo.ModuleGlobal mgs:
                    return context.DomainService.GetGlobalValue(mgs.VarId);

                // 스크립트
                case StorageInfo.PrivateGlobal pgs:
                    return context.GetPrivateGlobalValue(pgs.Index);

                // 함수
                case StorageInfo.Local ls:
                    return context.GetLocalValue(ls.Index);

                // enum값
                case StorageInfo.EnumElem ees:
                    return new EnumValue(ees.Name, Enumerable.Empty<(string, Value)>());

                case StorageInfo.StaticMember sms:
                    {
                        if (sms.ObjectInfo != null)
                        {
                            var objValue = evaluator.GetDefaultValue(sms.ObjectInfo.Value.TypeValue, context);
                            await evaluator.EvalExpAsync(sms.ObjectInfo.Value.Exp, objValue, context);
                        }

                        // object와는 별개로 static value를 가져온다
                        return context.GetStaticValue(sms.VarValue);
                    }

                case StorageInfo.InstanceMember ims:
                    {
                        var objValue = evaluator.GetDefaultValue(ims.ObjectTypeValue, context);
                        await evaluator.EvalExpAsync(ims.ObjectExp, objValue, context);

                        return evaluator.GetMemberValue(objValue, ims.VarName);
                    }

                default:
                    throw new NotImplementedException();
            };
        }

        async ValueTask<Value> GetValueAsync(Exp exp, EvalContext context)
        {
            switch(exp)
            {
                case ExternalGlobalVarExp egvExp:
                    throw new NotImplementedException();

                case PrivateGlobalVarExp pgvExp:
                    return context.GetPrivateGlobalValue(pgvExp.Name);

                case LocalVarExp localVarExp:
                    return context.GetLocalValue(localVarExp.Name);
                
                case StructMemberExp structMemberExp:
                    var instValue = (StructValue)await GetValueAsync(structMemberExp.Object, context);
                    return instValue.GetMemberValue(structMemberExp.MemberName);

                case ClassMemberExp classMemberExp:
                    var instValue = (ClassValue)await GetValueAsync(classMemberExp.Object, context);
                    return instValue.GetMemberValue(classMemberExp.MemberName);

                case EnumMemberExp enumMemberExp:
                    var instValue = (EnumValue)await GetValueAsync(enumMemberExp.Object, context);
                    return instValue.GetMemberValue(enumMemberExp.MemberName);

                default: 
                    throw new InvalidOperationException();
            }
        }

        void EvalExternalGlobalVarExp(ExternalGlobalVarExp exp, Value result, EvalContext context)
        {
            throw new NotImplementedException();
        }

        void EvalPrivateGlobalVarExp(PrivateGlobalVarExp exp, Value result, EvalContext context)
        {
            var globalValue = context.GetPrivateGlobalValue(exp.Name);
            result.SetValue(globalValue);
        }

        void EvalLocalVarExp(LocalVarExp exp, Value result, EvalContext context)
        {
            var localValue = context.GetLocalValue(exp.Name);
            result.SetValue(localValue);
        }        
        
        void EvalBoolLiteralExp(BoolLiteralExp boolLiteralExp, Value result, EvalContext context)
        {
            context.RuntimeModule.SetBool(result, boolLiteralExp.Value);
        }

        void EvalIntLiteralExp(IntLiteralExp intLiteralExp, Value result, EvalContext context)
        {
            context.RuntimeModule.SetInt(result, intLiteralExp.Value);
        }

        internal async ValueTask EvalStringExpAsync(StringExp stringExp, Value result, EvalContext context)
        {
            // stringExp는 element들의 concatenation
            var sb = new StringBuilder();
            foreach (var elem in stringExp.Elements)
            {
                switch (elem)
                {
                    case TextStringExpElement textElem:
                        sb.Append(textElem.Text);
                        break;

                    case ExpStringExpElement expElem:
                        {
                            var tempValue = evaluator.GetDefaultValue(expElem.ExpTypeValue, context);

                            await EvalAsync(expElem.Exp, tempValue, context);
                            var strValue = context.RuntimeModule.GetString(tempValue!);
                            sb.Append(strValue);
                            break;
                        }

                    default:
                        throw new InvalidOperationException();
                }
            }

            context.RuntimeModule.SetString(context.DomainService, result, sb.ToString());
        }

        async ValueTask EvalAssignExpAsync(AssignExp exp, Value result, EvalContext context)
        {
            var destValue = await GetValueAsync(exp.Dest, context);

            await EvalAsync(exp.Src, destValue, context);

            result.SetValue(destValue);
        }

        // ++x; => x.Inc(); result = x;
        async ValueTask EvalPrefixExpAsync(PrefixExp exp, Value result, EvalContext context)
        {
            var operand = await GetValueAsync(exp.Operand, context);


            await EvalAsync(exp.Exp, VoidValue.Instance, context);
            

        }

        async ValueTask EvalUnaryOpExpAssignAsync(UnaryOpExp exp, Value result, EvalContext context)
        {
            async ValueTask EvalDirectAsync(UnaryOpExpAssignInfo.Direct directInfo)
            {
                // postfix
                if (directInfo.bReturnPrevValue)
                {
                    var oldValue = evaluator.GetDefaultValue(directInfo.ValueTypeValue, context);
                    var operatorInst = context.DomainService.GetFuncInst(directInfo.OperatorValue);

                    // universal getValue 
                    var loc = await GetValueAsync(directInfo.StorageInfo, context);

                    oldValue.SetValue(loc);

                    await evaluator.EvaluateFuncInstAsync(null, operatorInst, new Value[] { oldValue }, loc, context);

                    result.SetValue(oldValue);
                }
                else
                {
                    var oldValue = evaluator.GetDefaultValue(directInfo.ValueTypeValue, context);
                    var operatorInst = context.DomainService.GetFuncInst(directInfo.OperatorValue);

                    var loc = await GetValueAsync(directInfo.StorageInfo, context);

                    oldValue.SetValue(loc);

                    await evaluator.EvaluateFuncInstAsync(null, operatorInst, new Value[] { oldValue }, loc, context);

                    result.SetValue(loc);
                }
            }

            async ValueTask EvalCallFuncAsync(UnaryOpExpAssignInfo.CallFunc callFuncInfo)
            {
                Value? thisValue = null;

                if (callFuncInfo.ObjectExp != null)
                {
                    thisValue = evaluator.GetDefaultValue(callFuncInfo.ObjectTypeValue!, context);
                    await evaluator.EvalExpAsync(callFuncInfo.ObjectExp, thisValue, context);
                }

                var argValues = new List<Value>(callFuncInfo.Arguments.Length);
                foreach (var arg in callFuncInfo.Arguments)
                {
                    var argValue = evaluator.GetDefaultValue(arg.TypeValue, context);
                    await evaluator.EvalExpAsync(arg.Exp, argValue, context);
                    argValues.Add(argValue);
                }

                // postfix
                if (callFuncInfo.bReturnPrevValue)
                {
                    var v0 = evaluator.GetDefaultValue(callFuncInfo.ValueTypeValue0, context);
                    var v1 = evaluator.GetDefaultValue(callFuncInfo.ValueTypeValue1, context);
                    var getterInst = context.DomainService.GetFuncInst(callFuncInfo.Getter);
                    var operatorInst = context.DomainService.GetFuncInst(callFuncInfo.Operator);
                    var setterInst = context.DomainService.GetFuncInst(callFuncInfo.Setter);

                    // v0 = obj.GetX()
                    await evaluator.EvaluateFuncInstAsync(thisValue, getterInst, argValues, v0, context);

                    // v1 = operatorInc(v0);
                    await evaluator.EvaluateFuncInstAsync(null, operatorInst, new Value[] { v0 }, v1, context);

                    // obj.SetX(v1)
                    argValues.Add(v1);
                    await evaluator.EvaluateFuncInstAsync(thisValue, setterInst, argValues, VoidValue.Instance, context);

                    // return v0
                    result.SetValue(v0);
                }
                else
                {
                    var v0 = evaluator.GetDefaultValue(callFuncInfo.ValueTypeValue0, context);
                    var v1 = evaluator.GetDefaultValue(callFuncInfo.ValueTypeValue1, context);
                    var getterInst = context.DomainService.GetFuncInst(callFuncInfo.Getter);
                    var operatorInst = context.DomainService.GetFuncInst(callFuncInfo.Operator);
                    var setterInst = context.DomainService.GetFuncInst(callFuncInfo.Setter);

                    // v0 = obj.GetX()
                    await evaluator.EvaluateFuncInstAsync(thisValue, getterInst, argValues, v0, context);

                    // v1 = operatorInc(v0);
                    await evaluator.EvaluateFuncInstAsync(null, operatorInst, new Value[] { v0 }, v1, context);

                    // obj.SetX(v1)
                    argValues.Add(v1);
                    await evaluator.EvaluateFuncInstAsync(thisValue, setterInst, argValues, VoidValue.Instance, context);

                    // return v1
                    result.SetValue(v1);
                }
            }

            var info = context.GetNodeInfo<UnaryOpExpAssignInfo>(exp);

            if (info is UnaryOpExpAssignInfo.Direct directInfo)
                await EvalDirectAsync(directInfo);
            else if (info is UnaryOpExpAssignInfo.CallFunc callFuncInfo)
                await EvalCallFuncAsync(callFuncInfo);
            else
                throw new InvalidOperationException();
        }

        async ValueTask EvalUnaryOpExpAsync(UnaryOpExp exp, Value result, EvalContext context)
        {
            switch (exp.Kind)
            {
                case UnaryOpKind.PostfixInc:  // i++                
                case UnaryOpKind.PostfixDec: // i--
                case UnaryOpKind.PrefixInc: // ++i
                case UnaryOpKind.PrefixDec: // --i
                    await EvalUnaryOpExpAssignAsync(exp, result, context);
                    return;

                case UnaryOpKind.LogicalNot:
                    {
                        // 같은 타입이니 result 재사용
                        await EvalAsync(exp.Operand, result, context);
                        var boolValue = context.RuntimeModule.GetBool(result);
                        context.RuntimeModule.SetBool(result, !boolValue);
                        return;
                    }

                case UnaryOpKind.Minus: // -i
                    {
                        // 타입이 같으므로 재사용
                        await EvalAsync(exp.Operand, result, context);
                        var intValue = context.RuntimeModule.GetInt(result);
                        context.RuntimeModule.SetInt(result, -intValue);
                        return;
                    }
            }

            throw new NotImplementedException();
        }

        private ValueTask EvalBinaryAssignExpAsync(BinaryOpExp exp, Value result, EvalContext context)
        {
            // 내부함수는 꼭 필요하지 않은 이상 한 단계 아래로 만들지 않기로

            // x = 3, e.m = 3, C.m = 3, ...
            async ValueTask EvalDirectAsync(BinaryOpExpAssignInfo.Direct directInfo)
            {
                var loc = await GetValueAsync(directInfo.StorageInfo, context);
                await evaluator.EvalExpAsync(exp.Operand1, loc, context);
                result.SetValue(loc);
            }
             
            // property, indexer
            async ValueTask EvalCallSetterAsync(BinaryOpExpAssignInfo.CallSetter callSetterInfo)
            {
                Value? thisValue = null;

                // 1. Object 부분 실행
                if (callSetterInfo.Object != null)
                {
                    thisValue = evaluator.GetDefaultValue(callSetterInfo.ObjectTypeValue!, context);
                    await evaluator.EvalExpAsync(callSetterInfo.Object, thisValue, context);
                }

                var argValues = new List<Value>(callSetterInfo.Arguments.Length);

                // 2. operand1 실행
                var value = evaluator.GetDefaultValue(callSetterInfo.ValueTypeValue, context);
                await evaluator.EvalExpAsync(exp.Operand1, value, context);

                // 3. set value 호출 Object.Setter(Operand1)
                foreach (var arg in callSetterInfo.Arguments)
                {
                    var argValue = evaluator.GetDefaultValue(arg.TypeValue, context);
                    await evaluator.EvalExpAsync(arg.Exp, argValue, context);
                    argValues.Add(argValue);
                }

                argValues.Add(value);

                var setterInst = context.DomainService.GetFuncInst(callSetterInfo.Setter);
                await evaluator.EvaluateFuncInstAsync(thisValue, setterInst, argValues, VoidValue.Instance, context);

                // 4. result는 operand1실행 결과
                result.SetValue(value);
            }

            // BODY
            var info = context.GetNodeInfo<BinaryOpExpAssignInfo>(exp);

            if (info is BinaryOpExpAssignInfo.Direct directInfo)
                return EvalDirectAsync(directInfo);
            else if (info is BinaryOpExpAssignInfo.CallSetter callSetterInfo)
                return EvalCallSetterAsync(callSetterInfo);
            else
                throw new InvalidOperationException();
        }

        async ValueTask EvalBinaryOpExpAsync(BinaryOpExp exp, Value result, EvalContext context)
        {
            switch (exp.Kind)
            {
                case BinaryOpKind.Multiply:
                    {
                        await EvalAsync(exp.Operand0, result, context);
                        var intValue0 = context.RuntimeModule.GetInt(result);

                        await EvalAsync(exp.Operand1, result, context);                        
                        var intValue1 = context.RuntimeModule.GetInt(result);

                        context.RuntimeModule.SetInt(result, intValue0 * intValue1);
                        return;
                    }

                case BinaryOpKind.Divide:
                    {
                        await EvalAsync(exp.Operand0, result, context);
                        var intValue0 = context.RuntimeModule.GetInt(result);

                        await EvalAsync(exp.Operand1, result, context);
                        var intValue1 = context.RuntimeModule.GetInt(result);

                        context.RuntimeModule.SetInt(result, intValue0 / intValue1);
                        return;
                    }

                case BinaryOpKind.Modulo:
                    {
                        await EvalAsync(exp.Operand0, result, context);
                        var intValue0 = context.RuntimeModule.GetInt(result);

                        await EvalAsync(exp.Operand1, result, context);
                        var intValue1 = context.RuntimeModule.GetInt(result);

                        context.RuntimeModule.SetInt(result, intValue0 % intValue1);
                        return;
                    }

                case BinaryOpKind.Add:
                    {
                        var info = context.GetNodeInfo<BinaryOpExpInfo>(exp);

                        // TODO: 이쪽은 operator+로 교체될 것이므로 임시로 하드코딩
                        if (info.Type == BinaryOpExpInfo.OpType.Integer)
                        {
                            await EvalAsync(exp.Operand0, result, context);
                            var intValue0 = context.RuntimeModule.GetInt(result);

                            await EvalAsync(exp.Operand1, result, context);
                            var intValue1 = context.RuntimeModule.GetInt(result);

                            context.RuntimeModule.SetInt(result, intValue0 + intValue1);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.String)
                        {
                            await EvalAsync(exp.Operand0, result, context);
                            var strValue0 = context.RuntimeModule.GetString(result);

                            await EvalAsync(exp.Operand1, result, context);
                            var strValue1 = context.RuntimeModule.GetString(result);

                            context.RuntimeModule.SetString(context.DomainService, result, strValue0 + strValue1);
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }

                case BinaryOpKind.Subtract:
                    {
                        await EvalAsync(exp.Operand0, result, context);
                        var intValue0 = context.RuntimeModule.GetInt(result);

                        await EvalAsync(exp.Operand1, result, context);
                        var intValue1 = context.RuntimeModule.GetInt(result);

                        context.RuntimeModule.SetInt(result, intValue0 - intValue1);
                        return;
                    }

                case BinaryOpKind.LessThan:
                    {
                        // TODO: 이쪽은 operator<로 교체될 것이므로 임시로 하드코딩
                        var info = context.GetNodeInfo<BinaryOpExpInfo>(exp);

                        if (info.Type == BinaryOpExpInfo.OpType.Integer)
                        {
                            // TODO: 이것도 지역변수로 할당해야 한다
                            var tempInt = context.RuntimeModule.MakeInt(0);

                            await EvalAsync(exp.Operand0, tempInt, context);
                            var intValue0 = context.RuntimeModule.GetInt(tempInt);

                            await EvalAsync(exp.Operand1, tempInt, context);
                            var intValue1 = context.RuntimeModule.GetInt(tempInt);

                            context.RuntimeModule.SetBool(result, intValue0 < intValue1);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.String)
                        {
                            var tempStr = context.RuntimeModule.MakeNullObject();

                            await EvalAsync(exp.Operand0, tempStr, context);
                            var strValue0 = context.RuntimeModule.GetString(tempStr);

                            await EvalAsync(exp.Operand1, tempStr, context);
                            var strValue1 = context.RuntimeModule.GetString(tempStr);

                            context.RuntimeModule.SetBool(result, strValue0.CompareTo(strValue1) < 0);
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }

                case BinaryOpKind.GreaterThan:
                    {
                        // TODO: 이쪽은 operator> 로 교체될 것이므로 임시로 하드코딩
                        var info = context.GetNodeInfo<BinaryOpExpInfo>(exp);

                        if (info.Type == BinaryOpExpInfo.OpType.Integer)
                        {
                            // TODO: 이것도 지역변수로 할당해야 한다
                            var tempInt = context.RuntimeModule.MakeInt(0);

                            await EvalAsync(exp.Operand0, tempInt, context);
                            var intValue0 = context.RuntimeModule.GetInt(tempInt);

                            await EvalAsync(exp.Operand1, tempInt, context);
                            var intValue1 = context.RuntimeModule.GetInt(tempInt);

                            context.RuntimeModule.SetBool(result, intValue0 > intValue1);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.String)
                        {
                            var tempStr = context.RuntimeModule.MakeNullObject();

                            await EvalAsync(exp.Operand0, tempStr, context);
                            var strValue0 = context.RuntimeModule.GetString(tempStr);

                            await EvalAsync(exp.Operand1, tempStr, context);
                            var strValue1 = context.RuntimeModule.GetString(tempStr);

                            context.RuntimeModule.SetBool(result, strValue0.CompareTo(strValue1) > 0);
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }

                case BinaryOpKind.LessThanOrEqual:
                    {
                        // TODO: 이쪽은 operator<=로 교체될 것이므로 임시로 하드코딩
                        var info = context.GetNodeInfo<BinaryOpExpInfo>(exp);

                        if (info.Type == BinaryOpExpInfo.OpType.Integer)
                        {
                            // TODO: 이것도 지역변수로 할당해야 한다
                            var tempInt = context.RuntimeModule.MakeInt(0);

                            await EvalAsync(exp.Operand0, tempInt, context);
                            var intValue0 = context.RuntimeModule.GetInt(tempInt);

                            await EvalAsync(exp.Operand1, tempInt, context);
                            var intValue1 = context.RuntimeModule.GetInt(tempInt);

                            context.RuntimeModule.SetBool(result, intValue0 <= intValue1);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.String)
                        {
                            var tempStr = context.RuntimeModule.MakeNullObject();

                            await EvalAsync(exp.Operand0, tempStr, context);
                            var strValue0 = context.RuntimeModule.GetString(tempStr);

                            await EvalAsync(exp.Operand1, tempStr, context);
                            var strValue1 = context.RuntimeModule.GetString(tempStr);

                            context.RuntimeModule.SetBool(result, strValue0.CompareTo(strValue1) <= 0);
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }

                case BinaryOpKind.GreaterThanOrEqual:
                    {
                        // TODO: 이쪽은 operator>=로 교체될 것이므로 임시로 하드코딩
                        var info = context.GetNodeInfo<BinaryOpExpInfo>(exp);

                        if (info.Type == BinaryOpExpInfo.OpType.Integer)
                        {
                            // TODO: 이것도 지역변수로 할당해야 한다
                            var tempInt = context.RuntimeModule.MakeInt(0);

                            await EvalAsync(exp.Operand0, tempInt, context);
                            var intValue0 = context.RuntimeModule.GetInt(tempInt);

                            await EvalAsync(exp.Operand1, tempInt, context);
                            var intValue1 = context.RuntimeModule.GetInt(tempInt);

                            context.RuntimeModule.SetBool(result, intValue0 >= intValue1);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.String)
                        {
                            var tempStr = context.RuntimeModule.MakeNullObject();

                            await EvalAsync(exp.Operand0, tempStr, context);
                            var strValue0 = context.RuntimeModule.GetString(tempStr);

                            await EvalAsync(exp.Operand1, tempStr, context);
                            var strValue1 = context.RuntimeModule.GetString(tempStr);

                            context.RuntimeModule.SetBool(result, strValue0.CompareTo(strValue1) >= 0);
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }

                case BinaryOpKind.Equal:
                    {
                        // TODO: 이쪽은 operator>=로 교체될 것이므로 임시로 하드코딩
                        var info = context.GetNodeInfo<BinaryOpExpInfo>(exp);

                        if (info.Type == BinaryOpExpInfo.OpType.Integer)
                        {
                            // TODO: 이것도 지역변수로 할당해야 한다
                            var tempInt = context.RuntimeModule.MakeInt(0);

                            await EvalAsync(exp.Operand0, tempInt, context);
                            var intValue0 = context.RuntimeModule.GetInt(tempInt);

                            await EvalAsync(exp.Operand1, tempInt, context);
                            var intValue1 = context.RuntimeModule.GetInt(tempInt);

                            context.RuntimeModule.SetBool(result, intValue0 == intValue1);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.String)
                        {
                            var tempStr = context.RuntimeModule.MakeNullObject();

                            await EvalAsync(exp.Operand0, tempStr, context);
                            var strValue0 = context.RuntimeModule.GetString(tempStr);

                            await EvalAsync(exp.Operand1, tempStr, context);
                            var strValue1 = context.RuntimeModule.GetString(tempStr);

                            context.RuntimeModule.SetBool(result, strValue0.CompareTo(strValue1) == 0);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.Bool)
                        {
                            var tempBool = context.RuntimeModule.MakeBool(false);

                            await EvalAsync(exp.Operand0, tempBool, context);
                            var boolValue0 = context.RuntimeModule.GetBool(tempBool);

                            await EvalAsync(exp.Operand1, tempBool, context);
                            var boolValue1 = context.RuntimeModule.GetBool(tempBool);

                            context.RuntimeModule.SetBool(result, boolValue0 == boolValue1);
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }

                case BinaryOpKind.NotEqual:
                    {
                        // TODO: 이쪽은 operator>=로 교체될 것이므로 임시로 하드코딩
                        var info = context.GetNodeInfo<BinaryOpExpInfo>(exp);

                        if (info.Type == BinaryOpExpInfo.OpType.Integer)
                        {
                            // TODO: 이것도 지역변수로 할당해야 한다
                            var tempInt = context.RuntimeModule.MakeInt(0);

                            await EvalAsync(exp.Operand0, tempInt, context);
                            var intValue0 = context.RuntimeModule.GetInt(tempInt);

                            await EvalAsync(exp.Operand1, tempInt, context);
                            var intValue1 = context.RuntimeModule.GetInt(tempInt);

                            context.RuntimeModule.SetBool(result, intValue0 != intValue1);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.String)
                        {
                            var tempStr = context.RuntimeModule.MakeNullObject();

                            await EvalAsync(exp.Operand0, tempStr, context);
                            var strValue0 = context.RuntimeModule.GetString(tempStr);

                            await EvalAsync(exp.Operand1, tempStr, context);
                            var strValue1 = context.RuntimeModule.GetString(tempStr);

                            context.RuntimeModule.SetBool(result, strValue0.CompareTo(strValue1) != 0);
                            return;
                        }
                        else if (info.Type == BinaryOpExpInfo.OpType.Bool)
                        {
                            var tempBool = context.RuntimeModule.MakeBool(false);

                            await EvalAsync(exp.Operand0, tempBool, context);
                            var boolValue0 = context.RuntimeModule.GetBool(tempBool);

                            await EvalAsync(exp.Operand1, tempBool, context);
                            var boolValue1 = context.RuntimeModule.GetBool(tempBool);

                            context.RuntimeModule.SetBool(result, boolValue0 != boolValue1);
                            return;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }

                case BinaryOpKind.Assign:
                    {
                        await EvalBinaryAssignExpAsync(exp, result, context);                        
                        return;
                    }
            }

            throw new NotImplementedException();
        }

        async ValueTask EvalCallFuncExpAsync(CallFuncExp exp, Value result, EvalContext context)
        {   
            var funcInst = context.DomainService.GetFuncInst(exp.FuncValue);

            Value? instValue = null;
            if (exp.Instance != null)
            {
                instValue = evaluator.GetDefaultValue(exp.Instance.Value.Type, context);
                await EvalAsync(exp.Instance.Value.Exp, instValue, context); 
            }

            var args = await EvalExpsAsync(exp.Args, context);

            await evaluator.EvaluateFuncInstAsync(funcInst.bThisCall ? instValue : null, funcInst, args, result, context);
        }

        async ValueTask EvalCallValueExpAsync(CallValueExp exp, Value result, EvalContext context)
        {
            var callableValue = (FuncInstValue)evaluator.GetDefaultValue(exp.CallableType, context);

            await EvalAsync(exp.CallableExp, callableValue, context);
            FuncInst funcInst = callableValue.FuncInst!;

            var args = await EvalExpsAsync(exp.Args, context);

            await evaluator.EvaluateFuncInstAsync(funcInst.bThisCall ? context.GetThisValue() : null, funcInst, args, result, context);
        }

        async ValueTask<List<Value>> EvalExpsAsync(IEnumerable<ExpAndType> expAndTypes, EvalContext context)
        {
            var args = new List<Value>();

            foreach (var expAndType in expAndTypes)
            {
                var argValue = evaluator.GetDefaultValue(expAndType.TypeValue, context);
                args.Add(argValue);

                await EvalAsync(expAndType.Exp, argValue, context);
            }

            return args;
        }

        async ValueTask EvalEnumExpAsync(EnumExp exp, Value result, EvalContext context)
        {
            var members = new List<(string, Value)>();

            foreach (var member in exp.Members)
            {
                var argValue = evaluator.GetDefaultValue(member.Type, context);
                members.Add((exp.Name, argValue));

                await EvalAsync(member.Exp, argValue, context);
            }
            
            ((EnumValue)result).SetValue(exp.Name, members);
        }        

        void EvalLambdaExp(LambdaExp exp, Value result, EvalContext context)
        {
            var captures = evaluator.MakeCaptures(exp.CaptureInfo.Captures, context);

            ((FuncInstValue)result).SetFuncInst(new ScriptFuncInst(
                null,
                false,
                exp.CaptureInfo.bCaptureThis ? context.GetThisValue() : null,
                captures,
                exp.Body));
        }

        // a[x] => a.Func(x)
        async ValueTask EvalIndexerExpAsync(IndexerExp exp, Value result, EvalContext context)
        {
            var thisValue = evaluator.GetDefaultValue(exp.ObjectType, context);
            var indexValue = evaluator.GetDefaultValue(exp.IndexType, context);
            
            await EvalAsync(exp.Object, thisValue, context);

            await EvalAsync(exp.Index, indexValue, context);
            var funcInst = context.DomainService.GetFuncInst(exp.FuncValue);

            await evaluator.EvaluateFuncInstAsync(thisValue, funcInst, new Value[] { indexValue }, result, context);
        }

        //async ValueTask EvalMemberCallExpAsync(MemberCallExp exp, Value result, EvalContext context)
        //{
        //    var info = context.GetNodeInfo<MemberCallExpInfo>(exp);

        //    switch (info)
        //    {
        //        case MemberCallExpInfo.InstanceFuncCall instanceFuncCall:
        //            {
        //                var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!, context);

        //                await EvalAsync(exp.Object, thisValue, context);
        //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);
        //                var funcInst = context.DomainService.GetFuncInst(instanceFuncCall.FuncValue);
        //                await evaluator.EvaluateFuncInstAsync(thisValue, funcInst, args, result, context);
        //                return;
        //            }

        //        case MemberCallExpInfo.StaticFuncCall staticFuncCall:
        //            {
        //                if (staticFuncCall.bEvaluateObject)                        
        //                {
        //                    var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!, context);
        //                    await EvalAsync(exp.Object, thisValue, context);
        //                }

        //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);
        //                var funcInst = context.DomainService.GetFuncInst(staticFuncCall.FuncValue);
        //                await evaluator.EvaluateFuncInstAsync(null, funcInst, args, result, context);
        //                return;
        //            }

        //        case MemberCallExpInfo.InstanceLambdaCall instanceLambdaCall:
        //            {
        //                var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!, context);

        //                await EvalAsync(exp.Object, thisValue, context);
        //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);
        //                var memberValue = evaluator.GetMemberValue(thisValue, instanceLambdaCall.VarName);
        //                var funcInst = ((FuncInstValue)memberValue).FuncInst!;
        //                await evaluator.EvaluateFuncInstAsync(thisValue, funcInst, args, result, context);
        //                return;
        //            }

        //        case MemberCallExpInfo.StaticLambdaCall staticLambdaCall:
        //            {
        //                if (staticLambdaCall.bEvaluateObject)
        //                {
        //                    var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!, context);
        //                    await EvalAsync(exp.Object, thisValue, context);
        //                }

        //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);                        
        //                var memberValue = context.GetStaticValue(staticLambdaCall.VarValue);
        //                var funcInst = ((FuncInstValue)memberValue).FuncInst!;
        //                await evaluator.EvaluateFuncInstAsync(null, funcInst, args, result, context);
        //                return;
        //            }

        //        case MemberCallExpInfo.EnumValue enumValueInfo:
        //            {
        //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);
        //                var memberValues = enumValueInfo.ElemInfo.FieldInfos.Zip(args, (fieldInfo, arg) => (fieldInfo.Name, arg));
        //                ((EnumValue)result).SetValue(enumValueInfo.ElemInfo.Name, memberValues);
        //                return;
        //            }

        //        default:
        //            throw new NotImplementedException();
        //    }

        //    async ValueTask<List<Value>> EvaluateArgsAsync(IEnumerable<TypeValue> argTypeValues, IReadOnlyCollection<Exp> argExps)
        //    {
        //        var argsBuilder = new List<Value>(argExps.Count);

        //        foreach (var (typeValue, argExp) in Zip(argTypeValues, argExps))
        //        {
        //            // 타입을 통해서 value를 만들어 낼 수 있는가..
        //            var argValue = evaluator.GetDefaultValue(typeValue, context);
        //            argsBuilder.Add(argValue);

        //            await EvalAsync(argExp, argValue, context);
        //        }

        //        return argsBuilder;
        //    }
        //}

        async ValueTask EvalMemberExpAsync(MemberExp exp, Value result, EvalContext context)
        {
            // TODO: namespace가 있으면 Global(N.x의 경우) 이 될수 있다.  
            // 지금은 Instance(obj.id) / Static(Type.id)으로 나눠진다
            var info = context.GetNodeInfo<MemberExpInfo>(exp);

            if (info is MemberExpInfo.Instance instanceKind)
            {
                var objValue = evaluator.GetDefaultValue(instanceKind.ObjectTypeValue, context);
                await EvalAsync(exp.Object, objValue, context);

                result.SetValue(evaluator.GetMemberValue(objValue, instanceKind.VarName));
            }
            else if (info is MemberExpInfo.Static staticKind)
            {
                if (staticKind.bEvaluateObject)
                {
                    var objValue = evaluator.GetDefaultValue(staticKind.ObjectTypeValue!, context);
                    await EvalAsync(exp.Object, objValue, context);
                }

                // object와는 별개로 static value를 가져온다
                result.SetValue(context.GetStaticValue(staticKind.VarValue));
            }
            else if (info is MemberExpInfo.EnumElem enumElemKind)
            {
                ((EnumValue)result).SetValue(enumElemKind.Name, Enumerable.Empty<(string, Value)>());
            }
            else if (info is MemberExpInfo.EnumElemField enumElemField)
            {
                var objValue = evaluator.GetDefaultValue(enumElemField.ObjectTypeValue, context);
                await EvalAsync(exp.Object, objValue, context);

                result.SetValue(((EnumValue)objValue).GetValue(exp.MemberName));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        async ValueTask EvalListExpAsync(ListExp exp, Value result, EvalContext context)
        {
            var elems = new List<Value>(exp.Elems.Length);

            foreach (var elemExp in exp.Elems)
            {
                var elemValue = evaluator.GetDefaultValue(exp.ElemType, context);
                elems.Add(elemValue);

                await EvalAsync(elemExp, elemValue, context);
            }

            context.RuntimeModule.SetList(context.DomainService, result, exp.ElemType, elems);
        }

        internal async ValueTask EvalAsync(Exp exp, Value result, EvalContext context)
        {
            switch(exp)
            {
                case ExternalGlobalVarExp egvExp: EvalExternalGlobalVarExp(egvExp, result, context); break;
                case PrivateGlobalVarExp pgvExp: EvalPrivateGlobalVarExp(pgvExp, result, context); break;
                case LocalVarExp localVarExp: EvalLocalVarExp(localVarExp, result, context); break;
                case BoolLiteralExp boolExp: EvalBoolLiteralExp(boolExp, result, context); break;
                case IntLiteralExp intExp: EvalIntLiteralExp(intExp, result, context); break;
                case StringExp stringExp: await EvalStringExpAsync(stringExp, result, context); break;
                case UnaryOpExp unaryOpExp: await EvalUnaryOpExpAsync(unaryOpExp, result, context); break;
                case BinaryOpExp binaryOpExp: await EvalBinaryOpExpAsync(binaryOpExp, result, context); break;
                case CallFuncExp callFuncExp: await EvalCallFuncExpAsync(callFuncExp, result, context); break;
                case CallValueExp callValueExp: await EvalCallValueExpAsync(callValueExp, result, context); break;
                case EnumExp enumExp: await EvalEnumExpAsync(enumExp, result, context); break;
                case LambdaExp lambdaExp: EvalLambdaExp(lambdaExp, result, context); break;
                case IndexerExp indexerExp: await EvalIndexerExpAsync(indexerExp, result, context); break;
                case MemberCallExp memberCallExp: await EvalMemberCallExpAsync(memberCallExp, result, context); break;
                case MemberExp memberExp: await EvalMemberExpAsync(memberExp, result, context); break;
                case ListExp listExp: await EvalListExpAsync(listExp, result, context); break;
                default:  throw new NotImplementedException();
            }
        }
    }
}