using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Citron.Infra.CollectionExtensions;
using Citron.Infra;
using Citron.Collections;

using R = Citron.IR0;
using Citron.Symbol;

namespace Citron
{
    partial struct IR0Evaluator
    {
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
            var value = await EvalLocAsync(loadExp.Loc);
            result.SetValue(value);
        }
        
        async ValueTask EvalStringExpAsync(R.StringExp stringExp, Value result)
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
                            var strValue = evalContext.AllocValue<StringValue>(TypeIds.String);
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

        // F<int>(); //
        // 
        // void G<X>(X x) {}
        // void F<T>() // T => int
        // {
        //     T t;     // t => int
        //     G<T>(t); // funcSymbol은 G<T>, G의 첫번째 인자는 int가 되어야 한다
        // }
        // 그냥 argument마다 해줘도 될 것 같다
        async ValueTask<ImmutableArray<Value>> EvalArgumentsAsync(IFuncSymbol funcSymbol, ImmutableArray<R.Argument> args)
        {
            var argsBuilder = ImmutableArray.CreateBuilder<Value>();

            // argument들을 할당할 공간을 만든다
            var argValuesBuilder = ImmutableArray.CreateBuilder<Value>();

            // 파라미터를 보고 만든다. params 파라미터라면 
            int parameterCount = funcSymbol.GetParameterCount();
            for(int i = 0; i < parameterCount; i++)
            {
                var param = funcSymbol.GetParameter(i);

                if (param.Kind == FuncParameterKind.Params)
                {
                    // TODO: 꼭 tuple이 아닐수도 있다
                    var tupleType = (TupleType)param.Type;
                    int memberVarCount = tupleType.GetMemberVarCount();
                    
                    for(int j = 0; j < memberVarCount; j++)
                    {
                        var memberVar = tupleType.GetMemberVar(j);
                        var argValue = evalContext.AllocValue(memberVar.GetDeclType());
                        argValuesBuilder.Add(argValue);
                    }
                }
                else if (param.Kind == FuncParameterKind.Ref)
                {   
                    var argValue = evalContext.AllocRefValue();
                    argValuesBuilder.Add(argValue);
                }
                else if (param.Kind == FuncParameterKind.Default)
                {
                    var argValue = evalContext.AllocValue(param.Type);
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
            for(int i = 0; i < parameterCount; i++)
            {
                var param = funcSymbol.GetParameter(i);

                if (param.Kind == FuncParameterKind.Params)
                {
                    // TODO: 꼭 tuple이 아닐수도 있다
                    var tupleType = (TupleTypeId)param.Type;
                    var tupleElems = ImmutableArray.Create(argValues, argValueIndex, tupleType.MemberVarIds.Length);

                    var tupleValue = new TupleValue(tupleElems);
                    argsBuilder.Add(tupleValue);

                    argValueIndex += tupleType.MemberVarIds.Length;
                }
                else
                {
                    argsBuilder.Add(argValues[argValueIndex]);

                    argValueIndex++;
                }
            }

            return argsBuilder.ToImmutable();
        }

        // Value에 넣어야 한다, 묶는 방법도 설명해야 한다
        // values: params까지 포함한 분절단위
        async ValueTask EvalArgumentsAsync(EnumElemValue elemValue, ImmutableArray<R.Argument> args)
        {
            // argument들을 순서대로 할당한다
            int argValueIndex = 0;
            foreach (var arg in args)                
            {
                switch (arg)
                {
                    case R.Argument.Normal normalArg:
                        await EvalExpAsync(normalArg.Exp, elemValue.GetMemberValue(argValueIndex));
                        argValueIndex++;
                        break;

                    // params가 들어있다면
                    case R.Argument.Params paramsArg:
                        // CitronVM단계에서는 시퀀셜하게 메모리를 던져줄 것이지만, C# 버전에서는 그렇게 못하므로
                        // ArgValues들을 가리키는 TupleValue를 임의로 생성하고 값을 저장하도록 한다
                        var builder = ImmutableArray.CreateBuilder<Value>(paramsArg.ElemCount - argValueIndex);
                        for (int i = argValueIndex; i < paramsArg.ElemCount; i++)
                            builder.Add(elemValue.GetMemberValue(i));
                        var tupleElems = builder.MoveToImmutable();

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

        // runtime typeContext |- EvalGlobalCallFuncExp(CallFuncExp(X<int>.Y<T>.Func<int>, 
        async ValueTask EvalCallGlobalFuncExpAsync(R.CallGlobalFuncExp exp, Value result)
        {
            // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
            var args = await EvalArgumentsAsync(exp.Func, exp.Args);
            await evalContext.ExecuteGlobalFuncAsync(exp.Func, args, result);
        }

        async ValueTask EvalCallClassMemberFuncExpAsync(R.CallClassMemberFuncExp exp, Value result)
        {
            // 함수는 this call이지만 instance가 없는 경우는 없다.
            Value? thisValue;
            if (exp.Instance != null)
                thisValue = await EvalLocAsync(exp.Instance);
            else
                thisValue = null;

            // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
            var args = await EvalArgumentsAsync(exp.ClassMemberFunc, exp.Args);
            await evalContext.ExecuteClassMemberFuncAsync(exp.ClassMemberFunc, thisValue, args, result);
        }
            
        async ValueTask EvalCallStructMemberFuncExpAsync(R.CallStructMemberFuncExp exp, Value result)
        {
            // 함수는 this call이지만 instance가 없는 경우는 없다.
            Value? thisValue;
            if (exp.Instance != null)
                thisValue = await EvalLocAsync(exp.Instance);
            else
                thisValue = null;

            // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
            var args = await EvalArgumentsAsync(exp.StructMemberFunc, exp.Args);

            await evalContext.ExecuteStructMemberFuncAsync(exp.StructMemberFunc, thisValue, args, result);
        }

        //async ValueTask EvalCallSeqFuncExpAsync(R.CallSeqFuncExp exp, Value result)
        //{
        //    var seqFuncItem = globalContext.GetRuntimeItem<SeqFuncRuntimeItem>(exp.SeqFunc);

        //    // 함수는 this call이지만 instance가 없는 경우는 없다.
        //    Debug.Assert(!(seqFuncItem.IsThisCall && exp.Instance == null));

        //    Value? thisValue = null;
        //    if (exp.Instance != null)
        //    {
        //        Debug.Assert(seqFuncItem.IsThisCall);
        //        thisValue = await EvalLocAsync(exp.Instance);
        //    }

        //    var args = await EvalArgumentsAsync(seqFuncItem.Parameters, exp.Args);
        //    seqFuncItem.Invoke(thisValue, args, result);
        //}

        // CallValue라기 보다 CallLambda (func<>는 invoke인터페이스 함수 호출로 미리 변경된다)
        async ValueTask EvalCallValueExpAsync(R.CallValueExp exp, Value result)
        {
            var lambda = exp.Lambda;

            var lambdaValue = (LambdaValue)await EvalLocAsync(exp.Callable);
            var localVars = await EvalArgumentsAsync(lambda, exp.Args);

            var builder = ImmutableDictionary.CreateBuilder<Name, Value>();
            // args는 로컬 변수로 등록된다
            for (int i = 0; i < localVars.Length; i++)
            {
                var parameter = lambda.GetParameter(i);
                builder.Add(parameter.Name, localVars[i]);
            }

            // Func<int>(); // 실행시점. TypeContext(T => int)
            // void Func<T>()
            // {
            //     T t;             // 실행시점. TypeContext(T => int)
            //     var l = () => t; // 실행시점. Func<T>.L0, TypeContext(T => int)
            //     l();             // 실행시점. l body의 typeContext는 지금 컨텍스트와 같다
            // }

            // typeContext를 
            // var typeContext = TypeContext.Make(lambda.GetSymbolId());

            var newContext = evalContext.NewLambdaContext(lambdaValue, result);
            var newLocalContext = new IR0LocalContext(builder.ToImmutable(), default);
            
            var body = globalContext.GetBodyStmt(lambda.GetSymbolId());

            var evaluator = new IR0Evaluator(globalContext, newContext, newLocalContext);
            await evaluator.EvalBodySkipYieldAsync(body);
        }

        // TODO: StmtEvaluator에서 사용, 합쳐지면 internal 뗀다
        internal async ValueTask EvalCaptureArgs(LambdaSymbol lambda, LambdaValue value, ImmutableArray<R.Argument> captureArgs)
        {
            // 1:1로 직접 꽂아보자
            var memberVarCount = lambda.GetMemberVarCount();
            Debug.Assert(captureArgs.Length == memberVarCount);
            
            for (int i = 0; i < memberVarCount; i++)
            {
                var memberVar = lambda.GetMemberVar(i);
                var memberValue = value.GetMemberValue(memberVar.GetName());
                
                var normalArg = captureArgs[i] as R.Argument.Normal;
                if (normalArg == null)
                    throw new NotImplementedException(); // 일단 normal만 지원

                await EvalExpAsync(normalArg.Exp, memberValue);
            }
        }
        
        ValueTask EvalLambdaExpAsync(R.LambdaExp exp, Value result_value)
        {
            LambdaValue result = (LambdaValue)result_value;
            return EvalCaptureArgs(exp.Lambda, result, exp.Args);
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
            var itemType = globalContext.GetListItemType(exp.ListType);
            if (itemType == null)
                throw new RuntimeFatalException();

            foreach (var elemExp in exp.Elems)
            {
                var elemValue = evalContext.AllocValue(itemType);
                list.Add(elemValue);

                await EvalExpAsync(elemExp, elemValue);
            }

            ((ListValue)result).SetList(list);
        }

        class ListSequence : ISequence
        {
            List<Value> values;
            int index;

            public ListSequence(List<Value> values)
            {
                this.values = values;
                this.index = 0;
            }

            public ValueTask<bool> MoveNextAsync(Value value)
            {
                if (index < values.Count)
                {
                    value.SetValue(values[index]);
                    index++;
                    return new ValueTask<bool>(true);
                }

                return new ValueTask<bool>(false);
            }
        }

        async ValueTask EvalListIterExpAsync(R.ListIteratorExp exp, Value result_value)
        {
            var listValue = (ListValue)await EvalLocAsync(exp.ListLoc);
            var result = (SeqValue)result_value;            

            var list = listValue.GetList();
            var sequence = new ListSequence(list);

            result.SetSequence(sequence);
        }

        async ValueTask EvalNewEnumExpAsync(R.NewEnumElemExp exp, Value result_value)
        {
            var result = (EnumElemValue)result_value;

            // 메모리 시퀀스                
            await EvalArgumentsAsync(result, exp.Args);
        }

        // 할당은 result에서 미리 하고, 여기서는 Constructor만 호출해준다
        async ValueTask EvalNewStructExpAsync(R.NewStructExp exp, Value result)
        {
            StructValue thisValue = (StructValue)result;
            var args = await EvalArgumentsAsync(exp.Constructor, exp.Args);
            evalContext.ExecuteStructConstructor(exp.Constructor, thisValue, args);
        }

        async ValueTask EvalNewClassExpAsync(R.NewClassExp exp, Value result_value)
        {
            // F<int>();  // 타입인자로 int를 주고 실행했을 때
            //
            // void F<T> // 함수 전체에 T => int 정보가 있다, 이 정보는 EvalContext에 있다
            // {
            //     var c = new C<T>(); // [T => int] C<T>, C<int>로 만들고, Alloc을 수행한다
            // }

            // 1. 인스턴스를 만들고
            var instance = evalContext.AllocClassInstance(exp.Constructor.GetOuter());

            // 2. 클래스 값에 넣은다음
            ClassValue thisValue = (ClassValue)result_value;
            thisValue.SetInstance(instance);

            // 3. 생성자 호출
            var args = await EvalArgumentsAsync(exp.Constructor, exp.Args);
            evalContext.ExecuteClassConstructor(exp.Constructor, thisValue, args);
        }

        // E e = (E)E.First;
        async ValueTask EvalCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp, Value result_value)
        {
            var result = (EnumValue)result_value;

            result.SetEnumElemId(castEnumElemToEnumExp.EnumElem.GetSymbolId());
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
                case R.CallInternalUnaryAssignOperatorExp ciuaoExp: await EvalCallInternalUnaryAssignOperatorExpAsync(ciuaoExp, result); break;
                case R.CallInternalBinaryOperatorExp ciboExp: await EvalCallInternalBinaryOperatorExpAsync(ciboExp, result); break;
                case R.AssignExp assignExp: await EvalAssignExpAsync(assignExp, result); break;
                case R.CallGlobalFuncExp callFuncExp: await EvalCallGlobalFuncExpAsync(callFuncExp, result); break;
                case R.CallClassMemberFuncExp ccmfe: await EvalCallClassMemberFuncExpAsync(ccmfe, result); break;
                case R.CallStructMemberFuncExp csmfe: await EvalCallStructMemberFuncExpAsync(csmfe, result); break;
                //case R.CallSeqFuncExp callSeqFuncExp: await EvalCallSeqFuncExpAsync(callSeqFuncExp, result); break;
                case R.CallValueExp callValueExp: await EvalCallValueExpAsync(callValueExp, result); break;
                case R.LambdaExp lambdaExp: await EvalLambdaExpAsync(lambdaExp, result); break;
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