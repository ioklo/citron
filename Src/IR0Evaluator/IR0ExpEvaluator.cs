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

using Citron.Symbol;
using Citron.IR0;

namespace Citron
{
    partial struct IR0ExpEvaluator : IIR0ExpVisitor<ValueTask>
    {
        IR0EvalContext context;
        Value result;        

        public IR0ExpEvaluator(IR0EvalContext context, Value result) 
        { 
            this.context = context;
            this.result = result; 
        }

        public static ValueTask EvalAsync(Exp exp, IR0EvalContext context, Value result)
        {
            var evaluator = new IR0ExpEvaluator(context, result);
            return exp.Accept<IR0ExpEvaluator, ValueTask>(ref evaluator);
        }

        public async ValueTask EvalStringAsync(StringExp exp)
        {
            // stringExp는 element들의 concatenation
            var sb = new StringBuilder();
            foreach (var elem in exp.Elements)
            {
                switch (elem)
                {
                    case TextStringExpElement textElem:
                        sb.Append(textElem.Text);
                        break;

                    case ExpStringExpElement expElem:
                        {
                            var strValue = context.AllocValue<StringValue>(TypeIds.String);
                            await EvalAsync(expElem.Exp, context, strValue);
                            sb.Append(strValue.GetString());
                            break;
                        }

                    default:
                        throw new UnreachableException();
                }
            }

            ((StringValue)result).SetString(sb.ToString());
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
        async ValueTask<ImmutableArray<Value>> EvalArgumentsAsync(IFuncSymbol funcSymbol, ImmutableArray<Argument> args)
        {
            var argsBuilder = ImmutableArray.CreateBuilder<Value>();

            // argument들을 할당할 공간을 만든다
            var argValuesBuilder = ImmutableArray.CreateBuilder<Value>();

            var funcDS = funcSymbol.GetDeclSymbolNode();

            // funcDS;
            int normalParamCount = funcSymbol.GetParameterCount();
            bool bLastParamVariadic = funcDS.IsLastParameterVariadic();

            if (bLastParamVariadic)
            {
                Debug.Assert(0 < normalParamCount);
                normalParamCount -= 1;
            }

            // 파라미터를 보고 만든다. params 파라미터라면             
            for(int i = 0; i < normalParamCount; i++)
            {
                var param = funcSymbol.GetParameter(i);
                var argValue = context.AllocValue(param.Type);
                argValuesBuilder.Add(argValue);
            }

            if (bLastParamVariadic)
            {
                var param = funcSymbol.GetParameter(normalParamCount);

                // TODO: [8] 꼭 tuple이 아닐수도 있다
                var tupleType = (TupleType)param.Type;
                int memberVarCount = tupleType.GetMemberVarCount();

                for (int j = 0; j < memberVarCount; j++)
                {
                    var memberVar = tupleType.GetMemberVar(j);
                    var argValue = context.AllocValue(memberVar.GetDeclType());
                    argValuesBuilder.Add(argValue);
                }
            }

            var argValues = argValuesBuilder.ToImmutable();

            // argument들을 순서대로 할당한다
            int argValueIndex = 0;
            foreach(var arg in args)
            {
                switch(arg)
                {
                    case Argument.Normal normalArg:
                        await EvalAsync(normalArg.Exp, context, argValues[argValueIndex]);
                        argValueIndex++;
                        break;

                    // params가 들어있다면
                    case Argument.Params paramsArg:
                        // GumVM단계에서는 시퀀셜하게 메모리를 던져줄 것이지만, C# 버전에서는 그렇게 못하므로
                        // ArgValues들을 가리키는 TupleValue를 임의로 생성하고 값을 저장하도록 한다
                        var tupleElems = ImmutableArray.Create(argValues, argValueIndex, paramsArg.ElemCount);
                        var tupleValue = new TupleValue(tupleElems);
                        await EvalAsync(paramsArg.Exp, context, tupleValue);
                        argValueIndex += paramsArg.ElemCount;
                        break;
                }
            }

            // param 단위로 다시 묶어야지
            for(int i = 0; i < normalParamCount; i++)
                argsBuilder.Add(argValues[i]);

            if (bLastParamVariadic)
            {   
                var param = funcSymbol.GetParameter(normalParamCount);

                // TODO: [8] 꼭 tuple이 아닐수도 있다
                var tupleType = (TupleType)param.Type;
                var tupleElems = ImmutableArray.Create(argValues, normalParamCount, tupleType.GetMemberVarCount());

                var tupleValue = new TupleValue(tupleElems);
                argsBuilder.Add(tupleValue);
            }

            return argsBuilder.ToImmutable();
        }

        // Value에 넣어야 한다, 묶는 방법도 설명해야 한다
        // values: params까지 포함한 분절단위
        async ValueTask EvalArgumentsAsync(EnumElemValue elemValue, ImmutableArray<Argument> args)
        {
            // argument들을 순서대로 할당한다
            int argValueIndex = 0;
            foreach (var arg in args)                
            {
                switch (arg)
                {
                    case Argument.Normal normalArg:
                        await EvalAsync(normalArg.Exp, context, elemValue.GetMemberValue(argValueIndex));
                        argValueIndex++;
                        break;

                    // params가 들어있다면
                    case Argument.Params paramsArg:
                        // CitronVM단계에서는 시퀀셜하게 메모리를 던져줄 것이지만, C# 버전에서는 그렇게 못하므로
                        // ArgValues들을 가리키는 TupleValue를 임의로 생성하고 값을 저장하도록 한다
                        var builder = ImmutableArray.CreateBuilder<Value>(paramsArg.ElemCount - argValueIndex);
                        for (int i = argValueIndex; i < paramsArg.ElemCount; i++)
                            builder.Add(elemValue.GetMemberValue(i));
                        var tupleElems = builder.MoveToImmutable();

                        var tupleValue = new TupleValue(tupleElems);
                        await EvalAsync(paramsArg.Exp, context, tupleValue);
                        argValueIndex += paramsArg.ElemCount;
                        break;

                    //case Argument.Ref refArg:
                    //    throw new NotImplementedException();
                    //    // argValueIndex++;

                }
            }
        }

        //async ValueTask EvalCallSeqFuncExpAsync(CallSeqFuncExp exp, Value result)
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

        // TODO: StmtEvaluator에서 사용, 합쳐지면 internal 뗀다
        public static async ValueTask EvalCaptureArgs(LambdaSymbol lambda, LambdaValue value, ImmutableArray<Argument> captureArgs, IR0EvalContext context)
        {
            // 1:1로 직접 꽂아보자
            var memberVarCount = lambda.GetMemberVarCount();
            Debug.Assert(captureArgs.Length == memberVarCount);
            
            for (int i = 0; i < memberVarCount; i++)
            {
                var memberVar = lambda.GetMemberVar(i);
                var memberValue = value.GetMemberValue(memberVar.GetName());
                
                var normalArg = captureArgs[i] as Argument.Normal;
                if (normalArg == null)
                    throw new NotImplementedException(); // 일단 normal만 지원

                await EvalAsync(normalArg.Exp, context, memberValue);
            }
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

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitLoad(LoadExp exp)
        {
            var value = await IR0LocEvaluator.EvalAsync(exp.Loc, context);
            result.SetValue(value);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitAssign(AssignExp exp)
        {
            var destValue = await IR0LocEvaluator.EvalAsync(exp.Dest, context);

            await EvalAsync(exp.Src, context, destValue);

            result.SetValue(destValue);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitBoxExp(BoxExp boxExp)
        {
            var value = context.AllocValue(boxExp.InnerExp.GetExpType());
            await EvalAsync(boxExp.InnerExp, context, value);

            ((BoxPtrValue)result).Set(value, value);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitStaticBoxRef(StaticBoxRefExp exp)
        {
            var value = await IR0LocEvaluator.EvalAsync(exp.Loc, context);
            ((BoxPtrValue)result).Set(null, value);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitClassMemberBoxRef(ClassMemberBoxRefExp exp)
        {
            var holder = await IR0LocEvaluator.EvalAsync(exp.holderLoc, context);
            var value = context.GetClassMemberValue((ClassValue)holder, exp.Symbol.GetSymbolId());

            ((BoxPtrValue)result).Set(holder, value);
        }

        // &(*pS).a = > (ps, S.a)
        async ValueTask IIR0ExpVisitor<ValueTask>.VisitStructIndirectMemberBoxRef(StructIndirectMemberBoxRefExp exp)
        {
            var holder = context.AllocValue<BoxPtrValue>(exp.holderExp.GetExpType().GetTypeId());
            await IR0ExpEvaluator.EvalAsync(exp.holderExp, context, holder);

            var value = context.GetStructMemberValue((StructValue)holder.GetTarget(), exp.Symbol.GetSymbolId());
            ((BoxPtrValue)result).Set(holder, value);
        }
        
        async ValueTask IIR0ExpVisitor<ValueTask>.VisitStructMemberBoxRef(StructMemberBoxRefExp exp)
        {
            var parent = context.AllocValue<BoxPtrValue>(exp.Parent.GetExpType().GetTypeId());
            await IR0ExpEvaluator.EvalAsync(exp.Parent, context, parent);

            var value = context.GetStructMemberValue((StructValue)parent.GetTarget(), exp.Symbol.GetSymbolId());
            ((BoxPtrValue)result).SetTarget(value);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitLocalRef(LocalRefExp exp)
        {
            var target = await IR0LocEvaluator.EvalAsync(exp.InnerLoc, context);
            ((LocalPtrValue)result).SetTarget(target);
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitBoolLiteral(BoolLiteralExp exp)
        {
            ((BoolValue)result).SetBool(exp.Value);
            return ValueTask.CompletedTask;
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitIntLiteral(IntLiteralExp exp)
        {
            ((IntValue)result).SetInt(exp.Value);
            return ValueTask.CompletedTask;
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitString(StringExp exp)
        {
            return EvalStringAsync(exp);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitList(ListExp exp)
        {
            var list = new List<Value>(exp.Elems.Length);
            var itemType = context.GetListItemType(exp.ListType);
            if (itemType == null)
                throw new RuntimeFatalException();

            foreach (var elemExp in exp.Elems)
            {
                var elemValue = context.AllocValue(itemType);
                list.Add(elemValue);

                await EvalAsync(elemExp, context, elemValue);
            }

            ((ListValue)result).SetList(list);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitListIterator(ListIteratorExp exp)
        {
            var listValue = (ListValue)await IR0LocEvaluator.EvalAsync(exp.ListLoc, context);
            var seqResult = (SeqValue)result;

            var list = listValue.GetList();
            var sequence = new ListSequence(list);

            seqResult.SetSequence(sequence);
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitCallInternalUnaryOperator(CallInternalUnaryOperatorExp exp)
        {
            switch (exp.Operator)
            {
                case InternalUnaryOperator.LogicalNot_Bool_Bool: return Operator_LogicalNot_Bool_Bool(exp.Operand, (BoolValue)result);
                case InternalUnaryOperator.UnaryMinus_Int_Int: return Operator_UnaryMinus_Int_Int(exp.Operand, (IntValue)result);

                case InternalUnaryOperator.ToString_Bool_String: return Operator_ToString_Bool_String(exp.Operand, (StringValue)result);
                case InternalUnaryOperator.ToString_Int_String: return Operator_ToString_Int_String(exp.Operand, (StringValue)result);

                default: throw new UnreachableException();
            }
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitCallInternalUnaryAssignOperator(CallInternalUnaryAssignOperatorExp exp)
        {
            // ++x
            var operand = await IR0LocEvaluator.EvalAsync(exp.Operand, context);

            switch (exp.Operator)
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

        ValueTask IIR0ExpVisitor<ValueTask>.VisitCallInternalBinaryOperator(CallInternalBinaryOperatorExp exp)
        {
            switch (exp.Operator)
            {
                case InternalBinaryOperator.Multiply_Int_Int_Int: return Operator_Multiply_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                case InternalBinaryOperator.Divide_Int_Int_Int: return Operator_Divide_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                case InternalBinaryOperator.Modulo_Int_Int_Int: return Operator_Modulo_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                case InternalBinaryOperator.Add_Int_Int_Int: return Operator_Add_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                case InternalBinaryOperator.Add_String_String_String: return Operator_Add_String_String_String(exp.Operand0, exp.Operand1, (StringValue)result);
                case InternalBinaryOperator.Subtract_Int_Int_Int: return Operator_Subtract_Int_Int_Int(exp.Operand0, exp.Operand1, (IntValue)result);
                case InternalBinaryOperator.LessThan_Int_Int_Bool: return Operator_LessThan_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.LessThan_String_String_Bool: return Operator_LessThan_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.GreaterThan_Int_Int_Bool: return Operator_GreaterThan_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.GreaterThan_String_String_Bool: return Operator_GreaterThan_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool: return Operator_LessThanOrEqual_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.LessThanOrEqual_String_String_Bool: return Operator_LessThanOrEqual_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool: return Operator_GreaterThanOrEqual_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool: return Operator_GreaterThanOrEqual_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.Equal_Int_Int_Bool: return Operator_Equal_Int_Int_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.Equal_Bool_Bool_Bool: return Operator_Equal_Bool_Bool_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                case InternalBinaryOperator.Equal_String_String_Bool: return Operator_Equal_String_String_Bool(exp.Operand0, exp.Operand1, (BoolValue)result);
                default: throw new UnreachableException();
            }
        }

        // runtime typeContext |- EvalGlobalCallFuncExp(CallFuncExp(X<int>.Y<T>.Func<int>, 
        async ValueTask IIR0ExpVisitor<ValueTask>.VisitCallGlobalFunc(CallGlobalFuncExp exp)
        {
            // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
            var args = await EvalArgumentsAsync(exp.Func, exp.Args);
            await context.ExecuteGlobalFuncAsync(exp.Func, args, result);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitNewClass(NewClassExp exp)
        {
            // F<int>();  // 타입인자로 int를 주고 실행했을 때
            //
            // void F<T> // 함수 전체에 T => int 정보가 있다, 이 정보는 EvalContext에 있다
            // {
            //     var c = new C<T>(); // [T => int] C<T>, C<int>로 만들고, Alloc을 수행한다
            // }

            // 1. 인스턴스를 만들고
            var instance = context.AllocClassInstance(exp.Constructor.GetOuter());

            // 2. 클래스 값에 넣은다음
            ClassValue thisValue = (ClassValue)result;
            thisValue.SetInstance(instance);

            // 3. 생성자 호출
            var args = await EvalArgumentsAsync(exp.Constructor, exp.Args);
            context.ExecuteClassConstructor(exp.Constructor, thisValue, args);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitCallClassMemberFunc(CallClassMemberFuncExp exp)
        {
            // 함수는 this call이지만 instance가 없는 경우는 없다.
            Value? thisValue;
            if (exp.Instance != null)
                thisValue = await IR0LocEvaluator.EvalAsync(exp.Instance, context);
            else
                thisValue = null;

            // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
            var args = await EvalArgumentsAsync(exp.ClassMemberFunc, exp.Args);
            await context.ExecuteClassMemberFuncAsync(exp.ClassMemberFunc, thisValue, args, result);
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitCastClass(CastClassExp exp)
        {
            throw new NotImplementedException();
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitNewStruct(NewStructExp exp)
        {
            // 할당은 result에서 미리 하고, 여기서는 Constructor만 호출해준다
            var thisValue = context.AllocValue<LocalPtrValue>(new LocalPtrTypeId(new StructType(exp.Constructor.GetOuter()).GetTypeId()));
            thisValue.SetTarget(result);

            var args = await EvalArgumentsAsync(exp.Constructor, exp.Args);
            context.ExecuteStructConstructor(exp.Constructor, thisValue, args);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitCallStructMemberFunc(CallStructMemberFuncExp exp)
        {
            // 함수는 this call이지만 instance가 없는 경우는 없다.
            Value? thisValue;
            if (exp.Instance != null)
                thisValue = await IR0LocEvaluator.EvalAsync(exp.Instance, context);
            else
                thisValue = null;

            // 인자를 계산 해서 처음 로컬 variable에 집어 넣는다
            var args = await EvalArgumentsAsync(exp.StructMemberFunc, exp.Args);

            await context.ExecuteStructMemberFuncAsync(exp.StructMemberFunc, thisValue, args, result);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitNewEnumElem(NewEnumElemExp exp)
        {
            var enumElemResult = (EnumElemValue)result;

            // 메모리 시퀀스                
            await EvalArgumentsAsync(enumElemResult, exp.Args);
        }

        async ValueTask IIR0ExpVisitor<ValueTask>.VisitCastEnumElemToEnum(CastEnumElemToEnumExp exp)
        {
            // E e = (E)E.First;
            var enumResult = (EnumValue)result;

            enumResult.SetEnumElemId(exp.Symbol.GetSymbolId());
            await EvalAsync(exp.Src, context, enumResult.GetElemValue());
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitNullableNullLiteral(NullableNullLiteralExp exp)
        {
            throw new NotImplementedException();
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitNewNullable(NewNullableExp exp)
        {
            throw new NotImplementedException();
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitLambda(LambdaExp exp)
        {
            LambdaValue lambdaResult = (LambdaValue)result;
            return EvalCaptureArgs(exp.Lambda, lambdaResult, exp.Args, context);
        }

        // CallValue라기 보다 CallLambda (func<>는 invoke인터페이스 함수 호출로 미리 변경된다)
        async ValueTask IIR0ExpVisitor<ValueTask>.VisitCallValue(CallLambdaExp exp)
        {
            var lambda = exp.Lambda;

            var lambdaValue = (LambdaValue)await IR0LocEvaluator.EvalAsync(exp.Callable, context);
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

            var body = context.GetBodyStmt(lambda.GetSymbolId());

            var newContext = context.NewLambdaContext(lambdaValue, result, builder.ToImmutable());

            var stmtEvaluator = new IR0StmtEvaluator(newContext);
            await stmtEvaluator.EvalBodySkipYieldAsync(body);
        }

        ValueTask IIR0ExpVisitor<ValueTask>.CastBoxedLambdaToFunc(CastBoxedLambdaToFuncExp exp)
        {
            throw new NotImplementedException();
        }

        ValueTask IIR0ExpVisitor<ValueTask>.VisitInlineBlock(InlineBlockExp exp)
        {
            throw new NotImplementedException();
        }
    }
}