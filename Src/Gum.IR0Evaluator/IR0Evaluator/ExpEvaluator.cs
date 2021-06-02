using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.IR0Evaluator.Evaluator;
using static Gum.Infra.CollectionExtensions;
using Gum;
using Gum.Infra;
using Gum.Collections;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        partial class ExpEvaluator
        {
            private Evaluator evaluator;

            public ExpEvaluator(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            void EvalBoolLiteralExp(R.BoolLiteralExp boolLiteralExp, Value result)
            {
                ((BoolValue)result).SetBool(boolLiteralExp.Value);
            }

            void EvalIntLiteralExp(R.IntLiteralExp intLiteralExp, Value result)
            {
                ((IntValue)result).SetInt(intLiteralExp.Value);
            }

            async ValueTask EvalLoadExpAsync(R.LoadExp loadExp, Value result)
            {
                var value = await evaluator.EvalLocAsync(loadExp.Loc);
                result.SetValue(value);
            }

            // expEvaluator에서 직접 호출하기 때문에 internal
            internal async ValueTask EvalStringExpAsync(R.StringExp stringExp, Value result)
            {
                // stringExp는 element들의 concatenation
                var sb = new StringBuilder();
                foreach (var elem in stringExp.Elements)
                {
                    switch (elem)
                    {
                        case R.TextStringExpElement textElem:
                            sb.Append(textElem.Text);
                            break;

                        case R.ExpStringExpElement expElem:
                            {
                                var strValue = evaluator.AllocValue<StringValue>(R.Path.String);
                                await EvalAsync(expElem.Exp, strValue);
                                sb.Append(strValue.GetString());
                                break;
                            }

                        default:
                            throw new UnreachableCodeException();
                    }
                }

                ((StringValue)result).SetString(sb.ToString());
            }

            async ValueTask EvalAssignExpAsync(R.AssignExp exp, Value result)
            {
                var destValue = await evaluator.EvalLocAsync(exp.Dest);

                await EvalAsync(exp.Src, destValue);

                result.SetValue(destValue);
            }

            async ValueTask<ImmutableDictionary<string, Value>> EvalArgumentsAsync(
                R.ParamInfo paramInfo,
                ImmutableArray<R.Argument> args)
            {
                var argsBuilder = ImmutableDictionary.CreateBuilder<string, Value>();

                // argument들을 할당할 공간을 만든다
                var argValuesBuilder = ImmutableArray.CreateBuilder<Value>();

                // 파라미터를 보고 만든다. params 파라미터라면 
                int paramIndex = 0;
                foreach (var param in paramInfo.Parameters)
                {
                    if (paramIndex == paramInfo.VariadicParamIndex)
                    { 
                        // TODO: 꼭 tuple이 아닐수도 있다
                        var tupleType = (R.Path.TupleType)param.Type;
                        foreach (var elem in tupleType.Elems)
                        {
                            var argValue = evaluator.AllocValue(elem.Type);
                            argValuesBuilder.Add(argValue);
                        }
                    }
                    else
                    {
                        var argValue = evaluator.AllocValue(param.Type);
                        argValuesBuilder.Add(argValue);
                    }

                    paramIndex++;
                }

                var argValues = argValuesBuilder.ToImmutable();

                // argument들을 순서대로 할당한다
                int argValueIndex = 0;
                foreach(var arg in args)
                {
                    switch(arg)
                    {
                        case R.Argument.Normal normalArg:
                            await EvalAsync(normalArg.Exp, argValues[argValueIndex]);
                            argValueIndex++;
                            break;

                        // params가 들어있다면
                        case R.Argument.Params paramsArg:
                            // GumVM단계에서는 시퀀셜하게 메모리를 던져줄 것이지만, C# 버전에서는 그렇게 못하므로
                            // ArgValues들을 가리키는 TupleValue를 임의로 생성하고 값을 저장하도록 한다
                            var tupleElems = ImmutableArray.Create(argValues, argValueIndex, paramsArg.ElemCount);

                            var tupleValue = new TupleValue(tupleElems);
                            await EvalAsync(paramsArg.Exp, tupleValue);
                            argValueIndex += paramsArg.ElemCount;
                            break;

                        case R.Argument.Ref refArg:
                            throw new NotImplementedException();
                            // argValueIndex++;

                    }
                }

                // param 단위로 다시 묶어야지
                argValueIndex = 0;
                paramIndex = 0;
                foreach(var param in paramInfo.Parameters)
                {   
                    if (paramIndex == paramInfo.VariadicParamIndex)
                    {
                        // TODO: 꼭 tuple이 아닐수도 있다
                        var tupleType = (R.Path.TupleType)param.Type;
                        var tupleElems = ImmutableArray.Create(argValues, argValueIndex, tupleType.Elems.Length);

                        var tupleValue = new TupleValue(tupleElems);
                        argsBuilder.Add(param.Name, tupleValue);

                        argValueIndex += tupleType.Elems.Length;
                        paramIndex++;
                    }
                    else
                    {
                        argsBuilder.Add(param.Name, argValues[argValueIndex]);

                        argValueIndex++;
                        paramIndex++;
                    }
                }

                return argsBuilder.ToImmutable();
            }

            // runtime typeContext |- EvalCallFuncExp(CallFuncExp(X<int>.Y<T>.Func<int>, 
            async ValueTask EvalCallFuncExpAsync(R.CallFuncExp exp, Value result)
            {
                // 1. 누가 FuncDecl들을 저장하고 있는가
                // 2. 필요한 값: DeclId (Body가 있는 곳), TypeContext를 만들기 위한 
                // 1) X<int, short>.Y<string>.F<bool>, 
                // 2) (declId, [[[int, short], string], bool])
                // 누가 정보를 더 많이 가지고 있는가; 1) 필요한가? 
                var funcInvoker = evaluator.context.GetRuntimeItem<FuncRuntimeItem>(exp.Func);

                // TODO: typeContext를 계산합니다

                // 함수는 this call이지만 instance가 없는 경우는 없다.
                Value? thisValue;
                if (exp.Instance != null)
                    thisValue = await evaluator.EvalLocAsync(exp.Instance);
                else
                    thisValue = null;

                // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
                var args = await EvalArgumentsAsync(funcInvoker.ParamInfo, exp.Args);

                await funcInvoker.InvokeAsync(evaluator, thisValue, args, result);
            }

            async ValueTask EvalCallSeqFuncExpAsync(R.CallSeqFuncExp exp, Value result)
            {
                var seqFuncItem = evaluator.context.GetRuntimeItem<SeqFuncRuntimeItem>(exp.SeqFunc);

                // 함수는 this call이지만 instance가 없는 경우는 없다.
                Debug.Assert(!(seqFuncItem.IsThisCall && exp.Instance == null));

                Value? thisValue = null;
                if (exp.Instance != null)
                {
                    Debug.Assert(seqFuncItem.IsThisCall);
                    thisValue = await evaluator.EvalLocAsync(exp.Instance);
                }

                var args = await EvalArgumentsAsync(seqFuncItem.ParamInfo, exp.Args);
                seqFuncItem.Invoke(evaluator, thisValue, args, result);
            }

            async ValueTask EvalCallValueExpAsync(R.CallValueExp exp, Value result)
            {
                var callableValue = (LambdaValue)await evaluator.EvalLocAsync(exp.Callable);
                var lambdaRuntimeItem = evaluator.context.GetRuntimeItem<LambdaRuntimeItem>(exp.Lambda);
                var localVars = await EvalArgumentsAsync(lambdaRuntimeItem.ParamInfo, exp.Args);

                await lambdaRuntimeItem.InvokeAsync(evaluator, callableValue.CapturedThis, callableValue.Captures, localVars, result);
            }

            void EvalLambdaExp(R.LambdaExp exp, Value result)
            {
                var lambdaRuntimeItem = evaluator.context.GetRuntimeItem<LambdaRuntimeItem>(exp.Lambda);
                lambdaRuntimeItem.Capture(evaluator, (LambdaValue)result);                
            }

            //async ValueTask EvalMemberCallExpAsync(MemberCallExp exp, Value result)
            //{
            //    var info = evaluator.context.GetNodeInfo<MemberCallExpInfo>(exp);

            //    switch (info)
            //    {
            //        case MemberCallExpInfo.InstanceFuncCall instanceFuncCall:
            //            {
            //                var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!);

            //                await EvalAsync(exp.Object, thisValue);
            //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);
            //                var funcInst = evaluator.context.DomainService.GetFuncInst(instanceFuncCall.FuncValue);
            //                await evaluator.EvaluateFuncInstAsync(thisValue, funcInst, args, result);
            //                return;
            //            }

            //        case MemberCallExpInfo.StaticFuncCall staticFuncCall:
            //            {
            //                if (staticFuncCall.bEvaluateObject)                        
            //                {
            //                    var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!);
            //                    await EvalAsync(exp.Object, thisValue);
            //                }

            //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);
            //                var funcInst = evaluator.context.DomainService.GetFuncInst(staticFuncCall.FuncValue);
            //                await evaluator.EvaluateFuncInstAsync(null, funcInst, args, result);
            //                return;
            //            }

            //        case MemberCallExpInfo.InstanceLambdaCall instanceLambdaCall:
            //            {
            //                var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!);

            //                await EvalAsync(exp.Object, thisValue);
            //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);
            //                var memberValue = evaluator.GetMemberValue(thisValue, instanceLambdaCall.VarName);
            //                var funcInst = ((FuncInstValue)memberValue).FuncInst!;
            //                await evaluator.EvaluateFuncInstAsync(thisValue, funcInst, args, result);
            //                return;
            //            }

            //        case MemberCallExpInfo.StaticLambdaCall staticLambdaCall:
            //            {
            //                if (staticLambdaCall.bEvaluateObject)
            //                {
            //                    var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!);
            //                    await EvalAsync(exp.Object, thisValue);
            //                }

            //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);                        
            //                var memberValue = evaluator.context.GetStaticValue(staticLambdaCall.VarValue);
            //                var funcInst = ((FuncInstValue)memberValue).FuncInst!;
            //                await evaluator.EvaluateFuncInstAsync(null, funcInst, args, result);
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
            //            var argValue = evaluator.GetDefaultValue(typeValue);
            //            argsBuilder.Add(argValue);

            //            await EvalAsync(argExp, argValue);
            //        }

            //        return argsBuilder;
            //    }
            //}

            async ValueTask EvalListExpAsync(R.ListExp exp, Value result)
            {
                var list = new List<Value>(exp.Elems.Length);

                foreach (var elemExp in exp.Elems)
                {
                    var elemValue = evaluator.AllocValue(exp.ElemType);
                    list.Add(elemValue);

                    await EvalAsync(elemExp, elemValue);
                }

                ((ListValue)result).SetList(list);
            }

            // Value에 넣어야 한다, 묶는 방법도 설명해야 한다
            // values: params까지 포함한 분절단위
            async ValueTask EvalArgumentsAsync(ImmutableArray<Value> values, ImmutableArray<R.Argument> args)
            {
                // argument들을 순서대로 할당한다
                int argValueIndex = 0;
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case R.Argument.Normal normalArg:
                            await EvalAsync(normalArg.Exp, values[argValueIndex]);
                            argValueIndex++;
                            break;

                        // params가 들어있다면
                        case R.Argument.Params paramsArg:
                            // GumVM단계에서는 시퀀셜하게 메모리를 던져줄 것이지만, C# 버전에서는 그렇게 못하므로
                            // ArgValues들을 가리키는 TupleValue를 임의로 생성하고 값을 저장하도록 한다
                            var tupleElems = ImmutableArray.Create(values, argValueIndex, paramsArg.ElemCount);

                            var tupleValue = new TupleValue(tupleElems);
                            await EvalAsync(paramsArg.Exp, tupleValue);
                            argValueIndex += paramsArg.ElemCount;
                            break;

                        case R.Argument.Ref refArg:
                            throw new NotImplementedException();
                            // argValueIndex++;

                    }
                }
            }

            async ValueTask EvalNewEnumExpAsync(R.NewEnumElemExp exp, Value result_value)
            {
                var result = (EnumElemValue)result_value;

                // 메모리 시퀀스                
                await EvalArgumentsAsync(result.Fields, exp.Args);
                
                //var builder = ImmutableArray.CreateBuilder<NamedValue>(exp.Members.Length);
                //foreach (var member in exp.Members)
                //{   
                //    var argValue = evaluator.AllocValue(member.ExpInfo.Type);
                //    builder.Add((exp.Name, argValue));

                //    await EvalAsync(member.ExpInfo.Exp, argValue);
                //}
                //((EnumValue)result).SetEnum(exp.Name, builder.MoveToImmutable());
            }

            ValueTask EvalNewStructExpAsync(R.NewStructExp exp, Value result)
            {
                throw new NotImplementedException();
            }

            ValueTask EvalNewClassExpAsync(R.NewClassExp exp, Value result)
            {
                throw new NotImplementedException();
            }

            // E e = (E)E.First;
            async ValueTask EvalCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp, Value result_value)
            {
                var result = (EnumValue)result_value;
                var enumElemItem = evaluator.context.GetRuntimeItem<EnumElemRuntimeItem>(castEnumElemToEnumExp.EnumElem);

                result.SetEnumElemItem(enumElemItem);
                await EvalAsync(castEnumElemToEnumExp.Src, result.GetElemValue());
            }

            ValueTask EvalCastClassExp(R.CastClassExp castClassExp, Value result)
            {
                throw new NotImplementedException();
            }

            internal async ValueTask EvalAsync(R.Exp exp, Value result)
            {
                switch (exp)
                {
                    case R.LoadExp loadExp: await EvalLoadExpAsync(loadExp, result); break;
                    case R.StringExp stringExp: await EvalStringExpAsync(stringExp, result); break;
                    case R.IntLiteralExp intExp: EvalIntLiteralExp(intExp, result); break;
                    case R.BoolLiteralExp boolExp: EvalBoolLiteralExp(boolExp, result); break;
                    case R.CallInternalUnaryOperatorExp ciuoExp: await EvalCallInternalUnaryOperatorExpAsync(ciuoExp, result); break;
                    case R.CallInternalUnaryAssignOperator ciuaoExp: await EvalCallInternalUnaryAssignOperatorExpAsync(ciuaoExp, result); break;
                    case R.CallInternalBinaryOperatorExp ciboExp: await EvalCallInternalBinaryOperatorExpAsync(ciboExp, result); break;
                    case R.AssignExp assignExp: await EvalAssignExpAsync(assignExp, result); break;
                    case R.CallFuncExp callFuncExp: await EvalCallFuncExpAsync(callFuncExp, result); break;
                    case R.CallSeqFuncExp callSeqFuncExp: await EvalCallSeqFuncExpAsync(callSeqFuncExp, result); break;
                    case R.CallValueExp callValueExp: await EvalCallValueExpAsync(callValueExp, result); break;
                    case R.LambdaExp lambdaExp: EvalLambdaExp(lambdaExp, result); break;
                    case R.ListExp listExp: await EvalListExpAsync(listExp, result); break;
                    case R.NewEnumElemExp enumExp: await EvalNewEnumExpAsync(enumExp, result); break;
                    case R.NewStructExp newStructExp: await EvalNewStructExpAsync(newStructExp, result); break;
                    case R.NewClassExp newClassExp: await EvalNewClassExpAsync(newClassExp, result); break;
                    case R.CastEnumElemToEnumExp castEnumElemToEnumExp: await EvalCastEnumElemToEnumExp(castEnumElemToEnumExp, result); break;
                    case R.CastClassExp castClassExp: await EvalCastClassExp(castClassExp, result); break;

                    default: throw new NotImplementedException();
                }
            }

            
        }
    }
}