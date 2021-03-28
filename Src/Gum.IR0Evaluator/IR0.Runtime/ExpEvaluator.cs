using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.IR0.Runtime.Evaluator;
using static Gum.Infra.CollectionExtensions;
using Gum;
using Gum.IR0;
using Gum.Infra;

namespace Gum.IR0.Runtime
{
    public partial class ExpEvaluator
    {
        private Evaluator evaluator;

        public ExpEvaluator(Evaluator evaluator)
        {
            this.evaluator = evaluator;
        }
        
        
        async ValueTask<Value> GetValueAsync(Exp exp, EvalContext context)
        {
            switch(exp)
            {
                case GlobalVarExp pgvExp:
                    return context.GetGlobalValue(pgvExp.Name);

                case LocalVarExp localVarExp:
                    return context.GetLocalValue(localVarExp.Name);

                case StaticMemberExp staticMemberExp:
                    var staticValue = (StaticValue)context.GetStaticValue(staticMemberExp.Type);
                    return staticValue.GetMemberValue(staticMemberExp.MemberName);
                
                case StructMemberExp structMemberExp:
                    var structValue = (StructValue)await GetValueAsync(structMemberExp.Instance, context);
                    return structValue.GetMemberValue(structMemberExp.MemberName);

                case ClassMemberExp classMemberExp:
                    var classValue = (ClassValue)await GetValueAsync(classMemberExp.Instance, context);
                    return classValue.GetMemberValue(classMemberExp.MemberName);

                case EnumMemberExp enumMemberExp:
                    var enumValue = (EnumValue)await GetValueAsync(enumMemberExp.Instance, context);
                    return enumValue.GetMemberValue(enumMemberExp.MemberName);

                default: 
                    throw new UnreachableCodeException();
            }
        }        

        void EvalGlobalVarExp(GlobalVarExp exp, Value result, EvalContext context)
        {
            var globalValue = context.GetGlobalValue(exp.Name);
            result.SetValue(globalValue);
        }

        void EvalLocalVarExp(LocalVarExp exp, Value result, EvalContext context)
        {
            var localValue = context.GetLocalValue(exp.Name);
            result.SetValue(localValue);
        }        

        void EvalStaticMemberExp(StaticMemberExp exp, Value result, EvalContext context)
        {
            var staticValue = (StaticValue)context.GetStaticValue(exp.Type);
            var memberValue = staticValue.GetMemberValue(exp.MemberName);
            result.SetValue(memberValue);
        }

        async ValueTask EvalStructMemberExpAsync(StructMemberExp exp, Value result, EvalContext context)
        {
            var structValue = (StructValue)await GetValueAsync(exp.Instance, context);
            var memberValue = structValue.GetMemberValue(exp.MemberName);
            result.SetValue(memberValue);
        }

        async ValueTask EvalClassMemberExpAsync(ClassMemberExp exp, Value result, EvalContext context)
        {
            var classValue = (ClassValue)await GetValueAsync(exp.Instance, context);
            var memberValue = classValue.GetMemberValue(exp.MemberName);
            result.SetValue(memberValue);
        }

        async ValueTask EvalEnumMemberExpAsync(EnumMemberExp exp, Value result, EvalContext context)
        {
            var enumValue = (EnumValue)await GetValueAsync(exp.Instance, context);
            var memberValue = enumValue.GetMemberValue(exp.MemberName);
            result.SetValue(memberValue);
        }

        void EvalBoolLiteralExp(BoolLiteralExp boolLiteralExp, Value result, EvalContext context)
        {
            ((BoolValue)result).SetBool(boolLiteralExp.Value);
        }

        void EvalIntLiteralExp(IntLiteralExp intLiteralExp, Value result, EvalContext context)
        {
            ((IntValue)result).SetInt(intLiteralExp.Value);
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
                            var strValue = evaluator.AllocValue<StringValue>(Type.String, context);
                            await EvalAsync(expElem.Exp, strValue, context);
                            sb.Append(strValue.GetString());
                            break;
                        }

                    default:
                        throw new UnreachableCodeException();
                }
            }

            ((StringValue)result).SetString(sb.ToString());
        }

        async ValueTask EvalAssignExpAsync(AssignExp exp, Value result, EvalContext context)
        {
            var destValue = await GetValueAsync(exp.Dest, context);

            await EvalAsync(exp.Src, destValue, context);

            result.SetValue(destValue);
        }        
        
        async ValueTask EvalCallFuncExpAsync(CallFuncExp exp, Value result, EvalContext context)
        {   
            var funcDecl = context.GetFuncDecl<NormalFuncDecl>(exp.Func.DeclId);

            // 함수는 this call이지만 instance가 없는 경우는 없다.
            Debug.Assert(!(funcDecl.IsThisCall && exp.Instance == null));

            Value? thisValue = null;
            if (exp.Instance != null)
            {
                var instValue = evaluator.AllocValue(exp.Instance.Value.Type, context);
                await EvalAsync(exp.Instance.Value.Exp, instValue, context);

                // this call인 경우만 세팅한다
                if (funcDecl.IsThisCall)
                    thisValue = instValue;
            }

            // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
            var args = await evaluator.EvalArgumentsAsync(funcDecl.ParamNames, exp.Args, context);

            await context.ExecInNewFuncFrameAsync(args, EvalFlowControl.None, ImmutableArray<Task>.Empty, thisValue, result, async () =>
            {
                await foreach (var _ in evaluator.EvalStmtAsync(funcDecl.Body, context)) { }
            });
        }

        async ValueTask EvalCallSeqFuncExpAsync(CallSeqFuncExp exp, Value result, EvalContext context)
        {
            var funcDecl = context.GetFuncDecl<SequenceFuncDecl>(exp.Func.DeclId);

            // 함수는 this call이지만 instance가 없는 경우는 없다.
            Debug.Assert(!(funcDecl.IsThisCall && exp.Instance == null));

            Value? thisValue = null;
            if (exp.Instance != null)
            {
                var instValue = evaluator.AllocValue(exp.Instance.Value.Type, context);
                await EvalAsync(exp.Instance.Value.Exp, instValue, context);

                // this call인 경우만 세팅한다
                if (funcDecl.IsThisCall)
                    thisValue = instValue;
            }

            var localVars = await evaluator.EvalArgumentsAsync(funcDecl.ParamNames, exp.Args, context);
            
            // yield에 사용할 공간
            var yieldValue = evaluator.AllocValue(funcDecl.ElemType, context);

            // context 복제
            var newContext = new EvalContext(
                context,
                localVars,
                EvalFlowControl.None,
                ImmutableArray<Task>.Empty,
                thisValue,
                yieldValue);

            // asyncEnum을 만들기 위해서 내부 함수를 씁니다
            async IAsyncEnumerable<Value> WrapAsyncEnum()
            {
                await foreach (var value in evaluator.EvalStmtAsync(funcDecl.Body, newContext))
                {
                    yield return value;
                }
            }

            var asyncEnum = WrapAsyncEnum();

            ((AsyncEnumerableValue)result).SetEnumerable(asyncEnum);
        }

        async ValueTask EvalCallValueExpAsync(CallValueExp exp, Value result, EvalContext context)
        {
            var callableValue = (LambdaValue)evaluator.AllocValue(exp.Callable.Type, context);

            await EvalAsync(exp.Callable.Exp, callableValue, context);
            var lambda = callableValue.GetLambda();

            await evaluator.EvalLambdaAsync(lambda, exp.Args, result, context);
        }        
        
        void EvalLambdaExp(LambdaExp exp, Value result, EvalContext context)
        {
            var captures = evaluator.MakeCaptures(exp.CaptureInfo.Captures, context);

            ((LambdaValue)result).SetLambda(new Lambda(
                exp.CaptureInfo.bShouldCaptureThis ? context.GetThisValue() : null,
                captures,
                exp.ParamNames,
                exp.Body));
        }

        // l[x]
        async ValueTask EvalListIndexerExpAsync(ListIndexerExp exp, Value result, EvalContext context)
        {
            var listValue = (ListValue)evaluator.AllocValue(exp.ListInfo.Type, context);
            var indexValue = (IntValue)evaluator.AllocValue(exp.IndexInfo.Type, context);
            
            await EvalAsync(exp.ListInfo.Exp, listValue, context);
            await EvalAsync(exp.IndexInfo.Exp, indexValue, context);

            var list = listValue.GetList();

            result.SetValue(list[indexValue.GetInt()]);
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

        async ValueTask EvalListExpAsync(ListExp exp, Value result, EvalContext context)
        {
            var list = new List<Value>(exp.Elems.Length);

            foreach (var elemExp in exp.Elems)
            {
                var elemValue = evaluator.AllocValue(exp.ElemType, context);
                list.Add(elemValue);

                await EvalAsync(elemExp, elemValue, context);
            }

            ((ListValue)result).SetList(list); 
        }

        async ValueTask EvalNewEnumExpAsync(NewEnumExp exp, Value result, EvalContext context)
        {
            var builder = ImmutableArray.CreateBuilder<NamedValue>(exp.Members.Length);

            foreach (var member in exp.Members)
            {
                var argValue = evaluator.AllocValue(member.ExpInfo.Type, context);
                builder.Add((exp.Name, argValue));

                await EvalAsync(member.ExpInfo.Exp, argValue, context);
            }

            ((EnumValue)result).SetEnum(exp.Name, builder.MoveToImmutable());
        }

        ValueTask EvalNewStructExpAsync(NewStructExp exp, Value result, EvalContext context)
        {
            throw new NotImplementedException();
        }

        ValueTask EvalNewClassExpAsync(NewClassExp exp, Value result, EvalContext context)
        {
            throw new NotImplementedException();
        }

        internal async ValueTask EvalAsync(Exp exp, Value result, EvalContext context)
        {
            switch(exp)
            {
                case GlobalVarExp pgvExp: EvalGlobalVarExp(pgvExp, result, context); break;
                case LocalVarExp localVarExp: EvalLocalVarExp(localVarExp, result, context); break;                
                case StringExp stringExp: await EvalStringExpAsync(stringExp, result, context); break;
                case IntLiteralExp intExp: EvalIntLiteralExp(intExp, result, context); break;
                case BoolLiteralExp boolExp: EvalBoolLiteralExp(boolExp, result, context); break;
                case CallInternalUnaryOperatorExp ciuoExp: await EvalCallInternalUnaryOperatorExpAsync(ciuoExp, result, context); break;
                case CallInternalUnaryAssignOperator ciuaoExp: await EvalCallInternalUnaryAssignOperatorExpAsync(ciuaoExp, result, context); break;
                case CallInternalBinaryOperatorExp ciboExp: await EvalCallInternalBinaryOperatorExpAsync(ciboExp, result, context); break;
                case AssignExp assignExp: await EvalAssignExpAsync(assignExp, result, context); break;                
                case CallFuncExp callFuncExp: await EvalCallFuncExpAsync(callFuncExp, result, context); break;
                case CallSeqFuncExp callSeqFuncExp: await EvalCallSeqFuncExpAsync(callSeqFuncExp, result, context); break;
                case CallValueExp callValueExp: await EvalCallValueExpAsync(callValueExp, result, context); break;
                case LambdaExp lambdaExp: EvalLambdaExp(lambdaExp, result, context); break;
                case ListIndexerExp indexerExp: await EvalListIndexerExpAsync(indexerExp, result, context); break;
                case StaticMemberExp staticMemberExp: EvalStaticMemberExp(staticMemberExp, result, context); break;
                case StructMemberExp structMemberExp: await EvalStructMemberExpAsync(structMemberExp, result, context); break;
                case ClassMemberExp classMemberExp: await EvalClassMemberExpAsync(classMemberExp, result, context); break;
                case EnumMemberExp enumMemberExp: await EvalEnumMemberExpAsync(enumMemberExp, result, context); break;
                case ListExp listExp: await EvalListExpAsync(listExp, result, context); break;
                case NewEnumExp enumExp: await EvalNewEnumExpAsync(enumExp, result, context); break;
                case NewStructExp newStructExp: await EvalNewStructExpAsync(newStructExp, result, context); break;
                case NewClassExp newClassExp: await EvalNewClassExpAsync(newClassExp, result, context); break;

                default:  throw new NotImplementedException();
            }
        }
    }
}