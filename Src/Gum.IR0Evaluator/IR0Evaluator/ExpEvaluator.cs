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
        partial struct ExpEvaluator
        {
            GlobalContext globalContext;
            EvalContext context;
            LocalContext localContext;

            public static ValueTask EvalAsync(GlobalContext globalContext, EvalContext context, LocalContext localContext, R.Exp exp, Value result)
            {
                var evaluator = new ExpEvaluator(globalContext, context, localContext);
                return evaluator.EvalExpAsync(exp, result);
            }

            public static ValueTask EvalStringExpAsync(GlobalContext globalContext, EvalContext context, LocalContext localContext, R.StringExp stringExp, Value result)
            {
                var evaluator = new ExpEvaluator(globalContext, context, localContext);
                return evaluator.EvalStringExpAsync(stringExp, result);
            }

            ExpEvaluator(GlobalContext globalContext, EvalContext context, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.context = context;
                this.localContext = localContext;
            }

            void EvalBoolLiteralExp(R.BoolLiteralExp boolLiteralExp, Value result)
            {
                ((BoolValue)result).SetBool(boolLiteralExp.Value);
            }

            void EvalIntLiteralExp(R.IntLiteralExp intLiteralExp, Value result)
            {
                ((IntValue)result).SetInt(intLiteralExp.Value);
            }

            ValueTask<Value> EvalLocAsync(R.Loc loc) => LocEvaluator.EvalAsync(globalContext, context, localContext, loc);

            async ValueTask EvalLoadExpAsync(R.LoadExp loadExp, Value result)
            {
                var value = await EvalLocAsync(loadExp.Loc);
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
                                var strValue = globalContext.AllocValue<StringValue>(R.Path.String);
                                await EvalExpAsync(expElem.Exp, strValue);
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
                var destValue = await EvalLocAsync(exp.Dest);

                await EvalExpAsync(exp.Src, destValue);

                result.SetValue(destValue);
            }

            async ValueTask<ImmutableArray<Value>> EvalArgumentsAsync(
                ImmutableArray<R.Param> parameters,
                ImmutableArray<R.Argument> args)
            {
                var argsBuilder = ImmutableArray.CreateBuilder<Value>();

                // argument들을 할당할 공간을 만든다
                var argValuesBuilder = ImmutableArray.CreateBuilder<Value>();

                // 파라미터를 보고 만든다. params 파라미터라면 
                foreach (var param in parameters)
                {
                    if (param.Kind == R.ParamKind.Params)
                    {
                        // TODO: 꼭 tuple이 아닐수도 있다
                        var tupleType = (R.Path.TupleType)param.Type;
                        foreach (var elem in tupleType.Elems)
                        {
                            var argValue = globalContext.AllocValue(elem.Type);
                            argValuesBuilder.Add(argValue);
                        }
                    }
                    else if (param.Kind == R.ParamKind.Ref)
                    {   
                        var argValue = globalContext.AllocRefValue();
                        argValuesBuilder.Add(argValue);
                    }
                    else if (param.Kind == R.ParamKind.Normal)
                    {
                        var argValue = globalContext.AllocValue(param.Type);
                        argValuesBuilder.Add(argValue);
                    }
                    else
                    {
                        throw new UnreachableCodeException();
                    }
                }

                var argValues = argValuesBuilder.ToImmutable();

                // argument들을 순서대로 할당한다
                int argValueIndex = 0;
                foreach(var arg in args)
                {
                    switch(arg)
                    {
                        case R.Argument.Normal normalArg:
                            await EvalExpAsync(normalArg.Exp, argValues[argValueIndex]);
                            argValueIndex++;
                            break;

                        // params가 들어있다면
                        case R.Argument.Params paramsArg:
                            // GumVM단계에서는 시퀀셜하게 메모리를 던져줄 것이지만, C# 버전에서는 그렇게 못하므로
                            // ArgValues들을 가리키는 TupleValue를 임의로 생성하고 값을 저장하도록 한다
                            var tupleElems = ImmutableArray.Create(argValues, argValueIndex, paramsArg.ElemCount);
                            var tupleValue = new TupleValue(tupleElems);
                            await EvalExpAsync(paramsArg.Exp, tupleValue);
                            argValueIndex += paramsArg.ElemCount;
                            break;

                        case R.Argument.Ref refArg:
                            var value = await EvalLocAsync(refArg.Loc);
                            var refValue = (RefValue)argValues[argValueIndex];
                            refValue.SetTarget(value);
                            argValueIndex++;
                            break;
                    }
                }

                // param 단위로 다시 묶어야지
                argValueIndex = 0;
                foreach(var param in parameters)
                {   
                    if (param.Kind == R.ParamKind.Params)
                    {
                        // TODO: 꼭 tuple이 아닐수도 있다
                        var tupleType = (R.Path.TupleType)param.Type;
                        var tupleElems = ImmutableArray.Create(argValues, argValueIndex, tupleType.Elems.Length);

                        var tupleValue = new TupleValue(tupleElems);
                        argsBuilder.Add(tupleValue);

                        argValueIndex += tupleType.Elems.Length;
                    }
                    else
                    {
                        argsBuilder.Add(argValues[argValueIndex]);

                        argValueIndex++;
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
                var funcInvoker = globalContext.GetRuntimeItem<FuncRuntimeItem>(exp.Func);

                // TODO: typeContext를 계산합니다

                // 함수는 this call이지만 instance가 없는 경우는 없다.
                Value? thisValue;
                if (exp.Instance != null)
                    thisValue = await EvalLocAsync(exp.Instance);
                else
                    thisValue = null;

                // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
                var args = await EvalArgumentsAsync(funcInvoker.Parameters, exp.Args);

                await funcInvoker.InvokeAsync(thisValue, args, result);
            }

            async ValueTask EvalCallSeqFuncExpAsync(R.CallSeqFuncExp exp, Value result)
            {
                var seqFuncItem = globalContext.GetRuntimeItem<SeqFuncRuntimeItem>(exp.SeqFunc);

                // 함수는 this call이지만 instance가 없는 경우는 없다.
                Debug.Assert(!(seqFuncItem.IsThisCall && exp.Instance == null));

                Value? thisValue = null;
                if (exp.Instance != null)
                {
                    Debug.Assert(seqFuncItem.IsThisCall);
                    thisValue = await EvalLocAsync(exp.Instance);
                }

                var args = await EvalArgumentsAsync(seqFuncItem.Parameters, exp.Args);
                seqFuncItem.Invoke(thisValue, args, result);
            }

            async ValueTask EvalCallValueExpAsync(R.CallValueExp exp, Value result)
            {
                var callableValue = (LambdaValue)await EvalLocAsync(exp.Callable);
                var lambdaRuntimeItem = globalContext.GetRuntimeItem<LambdaRuntimeItem>(exp.Lambda);
                var localVars = await EvalArgumentsAsync(lambdaRuntimeItem.Parameters, exp.Args);

                await lambdaRuntimeItem.InvokeAsync(callableValue.CapturedThis, callableValue.Captures, localVars, result);
            }

            void EvalLambdaExp(R.LambdaExp exp, Value result)
            {
                var lambdaRuntimeItem = globalContext.GetRuntimeItem<LambdaRuntimeItem>(exp.Lambda);
                lambdaRuntimeItem.Capture(context, localContext, (LambdaValue)result);                
            }

            //async ValueTask EvalMemberCallExpAsync(MemberCallExp exp, Value result)
            //{
            //    var info = context.GetNodeInfo<MemberCallExpInfo>(exp);

            //    switch (info)
            //    {
            //        case MemberCallExpInfo.InstanceFuncCall instanceFuncCall:
            //            {
            //                var thisValue = evaluator.GetDefaultValue(info.ObjectTypeValue!);

            //                await EvalAsync(exp.Object, thisValue);
            //                var args = await EvaluateArgsAsync(info.ArgTypeValues, exp.Args);
            //                var funcInst = context.DomainService.GetFuncInst(instanceFuncCall.FuncValue);
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
            //                var funcInst = context.DomainService.GetFuncInst(staticFuncCall.FuncValue);
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
            //                var memberValue = context.GetStaticValue(staticLambdaCall.VarValue);
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
                    var elemValue = globalContext.AllocValue(exp.ElemType);
                    list.Add(elemValue);

                    await EvalExpAsync(elemExp, elemValue);
                }

                ((ListValue)result).SetList(list);
            }

            async ValueTask EvalListIterExpAsync(R.ListIteratorExp exp, Value result_value)
            {
                var listValue = (ListValue)await EvalLocAsync(exp.ListLoc);
                var result = (SeqValue)result_value;
                
                // evaluator 복제
                var newContext = new EvalContext(default, EvalFlowControl.None, null, VoidValue.Instance);

                // asyncEnum을 만들기 위해서 내부 함수를 씁니다
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                async IAsyncEnumerator<Infra.Void> WrapAsyncEnum()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                {
                    var list = listValue.GetList();                    

                    foreach (var elem in list)
                    {
                        var yieldValue = newContext.GetYieldValue();
                        yieldValue.SetValue(elem); // 복사

                        yield return Infra.Void.Instance;
                    }
                }

                var enumerator = WrapAsyncEnum();
                result.SetEnumerator(enumerator, newContext);
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
                            await EvalExpAsync(normalArg.Exp, values[argValueIndex]);
                            argValueIndex++;
                            break;

                        // params가 들어있다면
                        case R.Argument.Params paramsArg:
                            // GumVM단계에서는 시퀀셜하게 메모리를 던져줄 것이지만, C# 버전에서는 그렇게 못하므로
                            // ArgValues들을 가리키는 TupleValue를 임의로 생성하고 값을 저장하도록 한다
                            var tupleElems = ImmutableArray.Create(values, argValueIndex, paramsArg.ElemCount);

                            var tupleValue = new TupleValue(tupleElems);
                            await EvalExpAsync(paramsArg.Exp, tupleValue);
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
            }

            // 할당은 result에서 미리 하고, 여기서는 Constructor만 호출해준다
            async ValueTask EvalNewStructExpAsync(R.NewStructExp exp, Value result)
            {
                var runtimeItem = globalContext.GetRuntimeItem<ConstructorRuntimeItem>(exp.Constructor);

                var args = await EvalArgumentsAsync(runtimeItem.Parameters, exp.Args);

                await runtimeItem.InvokeAsync(result, args);
            }

            async ValueTask EvalNewClassExpAsync(R.NewClassExp exp, Value result_value)
            {
                // 1. 인스턴스를 만들고
                var classItem = globalContext.GetRuntimeItem<ClassRuntimeItem>(exp.Class);
                var typeContext = TypeContext.Make(exp.Class);
                var instance = classItem.AllocInstance(typeContext);

                // 2. 클래스 값에 넣은다음
                ClassValue result = (ClassValue)result_value;
                result.SetInstance(instance);

                // 3. 생성자 호출
                var constructorPath = new R.Path.Nested(exp.Class, R.Name.Constructor.Instance, exp.ConstructorParamHash, default);
                var constructorItem = globalContext.GetRuntimeItem<ConstructorRuntimeItem>(constructorPath);
                var args = await EvalArgumentsAsync(constructorItem.Parameters, exp.Args);
                await constructorItem.InvokeAsync(result, args);
            }

            // E e = (E)E.First;
            async ValueTask EvalCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp, Value result_value)
            {
                var result = (EnumValue)result_value;
                var enumElemItem = globalContext.GetRuntimeItem<EnumElemRuntimeItem>(castEnumElemToEnumExp.EnumElem);

                result.SetEnumElemItem(enumElemItem);
                await EvalExpAsync(castEnumElemToEnumExp.Src, result.GetElemValue());
            }

            ValueTask EvalCastClassExp(R.CastClassExp castClassExp, Value result)
            {
                throw new NotImplementedException();
            }

            async ValueTask EvalExpAsync(R.Exp exp, Value result)
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
                    case R.ListIteratorExp listIterExp: await EvalListIterExpAsync(listIterExp, result); break;
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