using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using static Gum.IR0Translator.Analyzer;
using static Gum.Infra.CollectionExtensions;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;
using System.Diagnostics.Contracts;
using Gum.Infra;
using Gum.Collections;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {
        partial struct StmtAndExpAnalyzer
        {
            // x
            ExpResult AnalyzeIdExp(S.IdentifierExp idExp, ResolveHint resolveHint)
            {
                var typeArgs = GetTypeValues(idExp.TypeArgs);
                var resolver = new IdExpIdentifierResolver(idExp.Value, typeArgs, resolveHint, globalContext, callableContext, localContext);
                var result = resolver.Resolve();

                switch (result)
                {
                    case IdentifierResult.NotFound:
                        globalContext.AddFatalError(A2007_ResolveIdentifier_NotFound, idExp);
                        break;

                    case IdentifierResult.Error errorResult:
                        HandleErrorIdentifierResult(idExp, errorResult);
                        break;

                    case IdentifierResult.LocalVar localVarResult:
                        if (localVarResult.bNeedCapture)
                            callableContext.AddLambdaCapture(new LocalLambdaCapture(localVarResult.VarName, localVarResult.TypeValue));
                        return new ExpResult.Loc(new R.LocalVarLoc(localVarResult.VarName), localVarResult.TypeValue);

                    case IdentifierResult.GlobalVar globalVarResult:
                        return new ExpResult.Loc(new R.GlobalVarLoc(globalVarResult.VarName), globalVarResult.TypeValue);

                    case IdentifierResult.Funcs funcsResult:
                        if (funcsResult.IsInstanceFunc)
                        {
                            // this가 가능한지 체크는 IdentifierResolver에서 했다. TODO: 그래도 Assert걸어놓자
                            return new ExpResult.Funcs(funcsResult.Outer, funcsResult.FuncInfos, funcsResult.TypeArgs, R.ThisLoc.Instance);
                        }
                        else
                        {
                            return new ExpResult.Funcs(funcsResult.Outer, funcsResult.FuncInfos, funcsResult.TypeArgs, null);
                        }

                    case IdentifierResult.Type:
                        globalContext.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, idExp);
                        break;

                    case IdentifierResult.EnumElem enumElemResult:
                        if (enumElemResult.IsStandalone)      // 인자 없이 있는 것
                        {
                            throw new NotImplementedException();
                            // return new ExpResult(new NewEnumExp(enumElemResult.Name, Array.Empty<NewEnumExp.Elem>()), enumElem.EnumTypeValue);
                        }
                        else
                        {
                            // TODO: Func일때 감싸기
                            throw new NotImplementedException();
                        }
                }

                throw new UnreachableCodeException();
            }

            ExpResult.Exp AnalyzeBoolLiteralExp(S.BoolLiteralExp boolExp)
            {
                return new ExpResult.Exp(new R.BoolLiteralExp(boolExp.Value), globalContext.GetBoolType());
            }

            ExpResult.Exp AnalyzeIntLiteralExp(S.IntLiteralExp intExp)
            {
                return new ExpResult.Exp(new R.IntLiteralExp(intExp.Value), globalContext.GetIntType());
            }

            [AutoConstructor]
            partial struct StringExpResult
            {
                public R.StringExp Exp { get; }
                public TypeValue TypeValue { get; }
            }

            ExpResult.Exp AnalyzeStringExp(S.StringExp stringExp)
            {
                var bFatal = false;

                var builder = ImmutableArray.CreateBuilder<R.StringExpElement>();
                foreach (var elem in stringExp.Elements)
                {
                    try
                    {
                        var expElem = AnalyzeStringExpElement(elem);
                        builder.Add(expElem);
                    }
                    catch (FatalAnalyzeException)
                    {
                        bFatal = true;
                    }
                }

                if (bFatal)
                    throw new FatalAnalyzeException();

                return new ExpResult.Exp(new R.StringExp(builder.ToImmutable()), globalContext.GetStringType());
            }


            // int만 지원한다
            ExpResult.Exp AnalyzeIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
            {
                var operandResult = AnalyzeExp(operand, ResolveHint.None);

                if (operandResult is ExpResult.Loc locResult)
                {
                    // int type 검사
                    if (!globalContext.IsAssignable(globalContext.GetIntType(), locResult.TypeValue))
                        globalContext.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);

                    return new ExpResult.Exp(new R.CallInternalUnaryAssignOperator(op, locResult.Result), globalContext.GetIntType());
                }
                else
                {
                    globalContext.AddFatalError(A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand);
                    throw new UnreachableCodeException();
                }
            }

            ExpResult.Exp AnalyzeUnaryOpExp(S.UnaryOpExp unaryOpExp)
            {
                var operandResult = AnalyzeExp_Exp(unaryOpExp.Operand, ResolveHint.None);

                switch (unaryOpExp.Kind)
                {
                    case S.UnaryOpKind.LogicalNot:
                        {
                            if (!globalContext.IsAssignable(globalContext.GetBoolType(), operandResult.TypeValue))
                                globalContext.AddFatalError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, unaryOpExp.Operand);

                            return new ExpResult.Exp(
                                new R.CallInternalUnaryOperatorExp(
                                    R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                                    operandResult.Result
                                ),
                                globalContext.GetBoolType()
                            );
                        }

                    case S.UnaryOpKind.Minus:
                        {
                            if (!globalContext.IsAssignable(globalContext.GetIntType(), operandResult.TypeValue))
                                globalContext.AddFatalError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, unaryOpExp.Operand);

                            return new ExpResult.Exp(
                                new R.CallInternalUnaryOperatorExp(
                                    R.InternalUnaryOperator.UnaryMinus_Int_Int,
                                    operandResult.Result),
                                globalContext.GetIntType());
                        }

                    case S.UnaryOpKind.PostfixInc: // e.m++ 등
                        return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, R.InternalUnaryAssignOperator.PostfixInc_Int_Int);

                    case S.UnaryOpKind.PostfixDec:
                        return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, R.InternalUnaryAssignOperator.PostfixDec_Int_Int);

                    case S.UnaryOpKind.PrefixInc:
                        return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, R.InternalUnaryAssignOperator.PrefixInc_Int_Int);

                    case S.UnaryOpKind.PrefixDec:
                        return AnalyzeIntUnaryAssignExp(unaryOpExp.Operand, R.InternalUnaryAssignOperator.PrefixDec_Int_Int);

                    default:
                        throw new UnreachableCodeException();
                }
            }

            ExpResult.Exp AnalyzeAssignBinaryOpExp(S.BinaryOpExp exp)
            {
                // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
                var destResult = AnalyzeExp(exp.Operand0, ResolveHint.None);

                if (destResult is ExpResult.Loc destLocResult)
                {
                    var srcResult = AnalyzeExp_Exp(exp.Operand1, ResolveHint.None);

                    if (!globalContext.IsAssignable(destLocResult.TypeValue, srcResult.TypeValue))
                        globalContext.AddFatalError(A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType, exp);

                    return new ExpResult.Exp(new R.AssignExp(destLocResult.Result, srcResult.Result), destLocResult.TypeValue);
                }
                else
                {
                    globalContext.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                    throw new UnreachableCodeException();
                }
            }

            ExpResult.Exp AnalyzeBinaryOpExp(S.BinaryOpExp binaryOpExp)
            {
                var operandResult0 = AnalyzeExp_Exp(binaryOpExp.Operand0, ResolveHint.None);
                var operandResult1 = AnalyzeExp_Exp(binaryOpExp.Operand1, ResolveHint.None);

                // 1. Assign 먼저 처리
                if (binaryOpExp.Kind == S.BinaryOpKind.Assign)
                {
                    return AnalyzeAssignBinaryOpExp(binaryOpExp);
                }

                // 2. NotEqual 처리
                if (binaryOpExp.Kind == S.BinaryOpKind.NotEqual)
                {
                    var equalInfos = globalContext.GetBinaryOpInfos(S.BinaryOpKind.Equal);
                    foreach (var info in equalInfos)
                    {
                        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                        if (globalContext.IsAssignable(info.OperandType0, operandResult0.TypeValue) &&
                            globalContext.IsAssignable(info.OperandType1, operandResult1.TypeValue))
                        {
                            var equalExp = new R.CallInternalBinaryOperatorExp(
                                info.IR0Operator,
                                operandResult0.Result,
                                operandResult1.Result
                            );

                            return new ExpResult.Exp(
                                new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.LogicalNot_Bool_Bool, equalExp),
                                info.ResultType);
                        }
                    }
                }

                // 3. InternalOperator에서 검색            
                var matchedInfos = globalContext.GetBinaryOpInfos(binaryOpExp.Kind);
                foreach (var info in matchedInfos)
                {
                    // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                    if (globalContext.IsAssignable(info.OperandType0, operandResult0.TypeValue) &&
                        globalContext.IsAssignable(info.OperandType1, operandResult1.TypeValue))
                    {
                        return new ExpResult.Exp(
                            new R.CallInternalBinaryOperatorExp(
                                info.IR0Operator,
                                operandResult0.Result,
                                operandResult1.Result),
                            info.ResultType);
                    }
                }

                // Operator를 찾을 수 없습니다
                globalContext.AddFatalError(A0802_BinaryOp_OperatorNotFound, binaryOpExp);
                return default; // suppress CS0161
            }

            //ExpResult AnalyzeCallExpEnumElemCallable(M.EnumElemInfo enumElem, ImmutableArray<ExpResult> argResults, S.CallExp nodeForErrorReport)
            //{
            //    // 인자 개수가 맞는지 확인
            //    if (enumElem.FieldInfos.Length != argResults.Length)
            //        context.AddFatalError(A0903_CallExp_MismatchEnumConstructorArgCount, nodeForErrorReport, "enum인자 개수가 맞지 않습니다");

            //    var members = new List<NewEnumExp.Elem>();
            //    foreach (var (fieldInfo, argResult) in Zip(enumElem.FieldInfos, argResults))
            //    {
            //        var appliedTypeValue = context.Apply(enumElem.EnumTypeValue, fieldInfo.TypeValue);
            //        if (!context.IsAssignable(appliedTypeValue, argResult.TypeValue))
            //            context.AddFatalError(A0904_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType, nodeForErrorReport, "enum의 {0}번째 인자 형식이 맞지 않습니다");

            //        var typeId = context.GetType(argResult.TypeValue);
            //        members.Add(new NewEnumExp.Elem(fieldInfo.Name, new ExpInfo(argResult.Exp, typeId)));
            //    }

            //    return new ExpResult(new NewEnumExp(enumElem.Name.ToString(), members), enumElem.EnumTypeValue);
            //}

            [AutoConstructor]
            partial struct MatchArgsResult
            {
                public static readonly MatchArgsResult Invalid = new MatchArgsResult();

                public bool bMatch { get; }
                public bool bExactMatch { get; } // TypeInference를 사용하지 않은 경우                
                public ImmutableArray<R.Exp> Args { get; }
                public ImmutableArray<TypeValue> TypeArgs { get; }
            }

            // params를 확장한 Argument
            abstract record Argument
            {
                // 기본 아규먼트 
                public record Normal(S.Exp Exp) : Argument;

                // Params가 확장된 아규먼트
                public record Expanded(int ParamsIndex, int Index, TypeValue TypeValue) : Argument;

                public record Ref(S.Exp Exp) : Argument;
            }

            [AutoConstructor]
            partial struct ParamsInfo
            {
                public R.Exp Exp { get; }
            }

            [AutoConstructor]
            partial struct ArgumentInfo
            {
                public ImmutableArray<ParamsInfo> ParamsInfos { get; }
                public ImmutableArray<Argument> Arguments { get; }
            }

            ArgumentInfo ExpandArguments(ImmutableArray<S.Argument> sargs)
            {
                var paramsInfos = ImmutableArray.CreateBuilder<ParamsInfo>();
                var args = ImmutableArray.CreateBuilder<Argument>();
                foreach(var sarg in sargs)
                {
                    if (sarg.ArgumentModifier == S.ArgumentModifier.None)
                        args.Add(new Argument.Normal(sarg.Exp));
                    else if (sarg.ArgumentModifier == S.ArgumentModifier.Params)
                    {
                        // expanded argument는 먼저 타입을 알아내야 한다
                        var expResult = AnalyzeExp_Exp(sarg.Exp, ResolveHint.None);
                        
                        if (expResult.TypeValue is TupleTypeValue tupleTypeValue)
                        {
                            int index = 0;
                            foreach(var elemTypeValue in tupleTypeValue.ElemTypeValues)
                            {
                                args.Add(new Argument.Expanded(paramsInfos.Count, index, elemTypeValue));
                                index++;
                            }

                            paramsInfos.Add(new ParamsInfo(expResult.Result));
                        }
                        else
                        {
                            // 에러
                            globalContext.AddFatalError();
                            throw new UnreachableCodeException();
                        }
                    }
                    else if (sarg.ArgumentModifier == S.ArgumentModifier.Ref)
                    {
                        args.Add(new Argument.Ref(sargs.Exp));
                    }
                }

                return new ArgumentInfo(paramsInfos.ToImmutable(), args.ToImmutable());
            }

            // typeEnv는 funcInfo미 포함 타입정보
            // typeArgs가 충분하지 않을 수 있다. 나머지는 inference로 채운다
            MatchArgsResult MatchArguments(TypeEnv outerTypeEnv, ImmutableArray<TypeValue> typeArgs, M.FuncInfo funcInfo, ImmutableArray<S.Argument> sargs)
            {
                // 1. 아규먼트에서 params를 확장시킨다 (params에는 타입힌트를 적용하지 않고 먼저 평가한다)
                var argInfo = ExpandArguments(sargs);

                // 2. funcInfo에서 params 앞부분과 뒷부분으로 나누기

                // 인자 개수가 맞는지?
                // TODO: params 고려,
                // if (funcInfo.ParamTypes.Length != sargs.Length)
                //    return MatchArgsResult.Invalid;                

                // 인자 타입이 맞는지
                for (int i = 0; i < funcInfo.ParamInfo.Parameters.Length; i++)
                {
                    var typeValue = globalContext.GetTypeValueByMType(funcInfo.ParamInfo.Parameters[i].Type);
                    typeValue = typeValue.Apply_TypeValue(outerTypeEnv); // TODO: 일단 임시

                    var resolveHint = ResolveHint.Make(typeValue);
                    var expResult = AnalyzeExp_Exp(sargs[i], resolveHint);

                    if (!globalContext.IsAssignable(typeValue, expResult.TypeValue))
                        return MatchArgsResult.Invalid;
                    
                    // 매칭이 되었다면
                }


            }

            [AutoConstructor]
            partial struct MatchedFunc
            {
                public MatchArgsResult Result { get; }
                public M.FuncInfo FuncInfo { get; }                
                public GlobalContext GlobalContext { get; }
                public CallableContext CallableContext { get; }
                public LocalContext LocalContext { get; }
            }
            
            // CallExp분석에서 Callable이 Identifier인 경우 처리
            ExpResult AnalyzeCallExpFuncCallable(ExpResult.Funcs funcsResult, ImmutableArray<S.Exp> sargs)
            {
                // 함수 중 하나를 골라야 한다
                // 1. 인자 개수가 맞는지 (params도 고려)
                // 2. 인자 타입이 맞는지 (타입 인퍼런스도 고려)
                // 3. 맞는개 여러개라면 1) (타입 인퍼런스 < 고정 인자) 우선, 2) 각각 (타입인퍼런스된 함수, 고정 인자 함수) 도 여러개라면 에러                

                // TypeEnv작성                
                var outerTypeEnv = funcsResult.Outer.GetTypeEnv();

                // 여러 함수 중에서 인자가 맞는것을 선택해야 한다
                var exactCandidates = new Candidates<MatchedFunc?>();
                var restCandidates = new Candidates<MatchedFunc?>();

                // Type inference
                foreach (var funcInfo in funcsResult.FuncInfos)
                {   
                    var clonedAnalyzer = CloneAnalyzer();
                    var matchResult = clonedAnalyzer.MatchArguments(outerTypeEnv, funcsResult.TypeArgs, funcInfo, sargs);

                    if (!matchResult.bMatch) continue;

                    var matchedCandidate = new MatchedFunc(matchResult, funcInfo, clonedAnalyzer.globalContext, clonedAnalyzer.callableContext, clonedAnalyzer.localContext);
                    
                    if (matchResult.bExactMatch)
                        exactCandidates.Add(matchedCandidate); // context만 저장하게 수정
                    else 
                        restCandidates.Add(matchedCandidate);
                }

                // 매칭 된 것으로
                MatchedFunc matchedFunc;
                var exactMatch = exactCandidates.GetSingle();
                if (exactMatch != null)
                {
                    matchedFunc = exactMatch.Value;
                }
                else if (exactCandidates.HasMultiple)
                {
                    globalContext.AddFatalError();
                    throw new UnreachableCodeException();
                }
                else // empty
                {
                    Debug.Assert(exactCandidates.IsEmpty);

                    var restMatch = restCandidates.GetSingle();
                    if (restMatch != null)
                    {
                        matchedFunc = restMatch.Value;
                    }
                    else if (restCandidates.HasMultiple)
                    {
                        globalContext.AddFatalError();
                        throw new UnreachableCodeException();
                    }
                    else // empty
                    {
                        globalContext.AddFatalError(); // No matched                 
                        throw new UnreachableCodeException();
                    }
                }

                // funcValue만들기
                var funcValue = globalContext.MakeFuncValue(funcsResult.Outer, matchedFunc.FuncInfo, matchedFunc.Result.TypeArgs);
                
                // context 업데이트
                UpdateAnalyzer(matchedFunc.GlobalContext, matchedFunc.CallableContext, matchedFunc.LocalContext);

                if (funcValue.IsSequence)
                {
                    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
                    var seqTypeValue = globalContext.GetSeqTypeValue(funcValue.GetRPath_Nested(), funcValue.GetRetType());
                    return new ExpResult.Exp(new R.CallSeqFuncExp(funcValue.GetRPath_Nested(), funcsResult.Instance, matchedFunc.Result.Args), seqTypeValue);
                }
                else
                {
                    // 만약
                    return new ExpResult.Exp(new R.CallFuncExp(funcValue.GetRPath_Nested(), funcsResult.Instance, matchedFunc.Result.Args), funcValue.GetRetType());
                }
            }

            // CallExp 분석에서 Callable이 Exp인 경우 처리
            ExpResult.Exp AnalyzeCallExpExpCallable(R.Loc callableLoc, TypeValue callableTypeValue, ImmutableArray<S.Exp> sargs, S.CallExp nodeForErrorReport)
            {
                var lambdaType = callableTypeValue as LambdaTypeValue;
                if (lambdaType == null)
                    globalContext.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, nodeForErrorReport.Callable);

                var argResultsBuilder = ImmutableArray.CreateBuilder<ExpResult.Exp>(sargs.Length);
                foreach (var arg in sargs)
                {
                    var expResult = AnalyzeExp_Exp(arg, ResolveHint.None);
                    argResultsBuilder.Add(expResult);
                }
                var argResults = argResultsBuilder.MoveToImmutable();

                var argTypes = ImmutableArray.CreateRange(argResults, info => info.TypeValue);
                CheckParamTypes(nodeForErrorReport, lambdaType.Params, argTypes);

                var rargs = argResults.Select(info => info.Result).ToImmutableArray();

                return new ExpResult.Exp(
                    new R.CallValueExp(lambdaType.Lambda, callableLoc, rargs),
                    lambdaType.Return);
            }

            ExpResult AnalyzeCallExp(S.CallExp exp, ResolveHint hint)
            {
                // 여기서 분석해야 할 것은 
                // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
                // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
                // 3. 잘 들어갔다면 리턴타입 -> 완료


                // E e = First(2, 3); => E e = E.First(2, 3);
                // EnumHint는 어떤 모습이어야 하나
                ResolveHint callableHint;
                if (hint.TypeHint is TypeValueTypeHint typeValueTypeHint && typeValueTypeHint.TypeValue is EnumTypeValue enumTypeValue)
                    callableHint = new ResolveHint(new EnumConstructorTypeHint(enumTypeValue));
                else
                    callableHint = ResolveHint.None;

                var callableResult = AnalyzeExp(exp.Callable, callableHint);

                // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
                // 함수 이름을 먼저 찾는가
                // Argument 타입을 먼저 알아내야 하는가
                // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다

                switch (callableResult)
                {
                    case ExpResult.Namespace:
                        globalContext.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, exp.Callable);
                        break;

                    // callable이 타입으로 계산되면, 에러
                    case ExpResult.Type:
                        globalContext.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, exp.Callable);
                        break;                        

                    case ExpResult.Funcs funcsResult:
                        return AnalyzeCallExpFuncCallable(funcsResult, exp.Args);

                    case ExpResult.Exp expResult:
                        var tempLoc = new R.TempLoc(expResult.Result, expResult.TypeValue.GetRPath());
                        return AnalyzeCallExpExpCallable(tempLoc, expResult.TypeValue, exp.Args, exp);

                    case ExpResult.Loc locResult:
                        return AnalyzeCallExpExpCallable(locResult.Result, locResult.TypeValue, exp.Args, exp);

                    // enum constructor, E.First
                    case ExpResult.EnumElem enumElemResult:
                        throw new NotImplementedException();
                        // return AnalyzeCallExpEnumElemCallable(enumElemResult.Info, );
                }

                throw new UnreachableCodeException();
            }

            ExpResult.Exp AnalyzeLambdaExp(S.LambdaExp exp)
            {
                // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
                TypeValue? retTypeValue = null;

                // 파라미터는 람다 함수의 지역변수로 취급한다
                var paramInfos = new List<(string Name, TypeValue TypeValue)>();
                foreach (var param in exp.Params)
                {
                    if (param.Type == null)
                        globalContext.AddFatalError(A9901_NotSupported_LambdaParameterInference, exp);

                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    paramInfos.Add((param.Name, paramTypeValue));
                }

                var newLambdaId = callableContext.NewAnonymousId();
                var newLambdaContext = new LambdaContext(callableContext, localContext, newLambdaId, retTypeValue);
                var newLocalContext = new LocalContext();
                var newAnalyzer = new StmtAndExpAnalyzer(globalContext, newLambdaContext, newLocalContext);

                // 람다 파라미터를 지역 변수로 추가한다
                foreach (var paramInfo in paramInfos)
                    newLocalContext.AddLocalVarInfo(paramInfo.Name, paramInfo.TypeValue);

                // 본문 분석
                var bodyResult = newAnalyzer.AnalyzeStmt(exp.Body);

                // 성공했으면, 리턴 타입 갱신            
                var capturedLocalVars = newLambdaContext.GetCapturedLocalVars();

                var paramTypes = paramInfos.Select(paramInfo => paramInfo.TypeValue).ToImmutableArray();
                var rparamInfos = paramInfos.Select(paramInfo => new R.ParamInfo(paramInfo.TypeValue.GetRPath(), paramInfo.Name)).ToImmutableArray();

                // TODO: need capture this확인해서 this 넣기
                // var bCaptureThis = newLambdaContext.NeedCaptureThis();
                R.Path? capturedThisType = null;

                var capturedStmt = new R.CapturedStatement(capturedThisType, capturedLocalVars, bodyResult.Stmt);
                var lambdaDecl = new R.LambdaDecl(new R.Name.Anonymous(newLambdaId), capturedStmt, rparamInfos);
                callableContext.AddDecl(lambdaDecl);

                var lambdaTypeValue = globalContext.GetLambdaTypeValue(
                    callableContext.GetPath(new R.Name.Anonymous(newLambdaId), R.ParamHash.None, default), // lambda는 paramHash를 넣지 않는다
                    newLambdaContext.GetRetTypeValue() ?? globalContext.GetVoidType(),
                    paramTypes
                );

                return new ExpResult.Exp(
                    new R.LambdaExp(lambdaTypeValue.Lambda),
                    lambdaTypeValue);
            }

            ExpResult.Exp AnalyzeIndexerExp(S.IndexerExp exp)
            {
                throw new NotImplementedException();

                //outExp = null;
                //outTypeValue = null;

                //if (!AnalyzeExp(exp.Object, null, out var obj, out var objType))
                //    return false;

                //if (!AnalyzeExp(exp.Index, null, out var index, out var indexType))
                //    return false;

                //// objTypeValue에 indexTypeValue를 인자로 갖고 있는 indexer가 있는지
                //if (!context.TypeValueService.GetMemberFuncValue(objType, SpecialNames.IndexerGet, ImmutableArray<TypeValue>.Empty, out var funcValue))
                //{
                //    context.ErrorCollector.Add(exp, "객체에 indexer함수가 없습니다");
                //    return false;
                //}

                //if (IsFuncStatic(funcValue.FuncId))
                //{
                //    Debug.Fail("객체에 indexer가 있는데 Static입니다");
                //    return false;
                //}

                //var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

                //if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, new[] { indexType }))
                //    return false;

                //var listType = analyzer.GetListTypeValue()

                //// List타입인가 확인
                //if (analyzer.IsAssignable(listType, objType))
                //{
                //    var objTypeId = context.GetTypeId(objType);
                //    var indexTypeId = context.GetTypeId(indexType);

                //    outExp = new ListIndexerExp(new ExpInfo(obj, objTypeId), new ExpInfo(index, indexTypeId));
                //    outTypeValue = funcTypeValue.Return;
                //    return true;
                //}
            }

            R.Loc MakeMemberLoc(TypeValue typeValue, R.Loc instance, string name)
            {
                switch (typeValue)
                {
                    case StructTypeValue structType:
                        return new R.StructMemberLoc(instance, name);
                }

                throw new NotImplementedException();
            }

            // exp.x
            ExpResult.Exp AnalyzeMemberExpExpParent(S.MemberExp memberExp, R.Loc parentLoc, TypeValue parentTypeValue)
            {
                var typeArgs = GetTypeValues(memberExp.MemberTypeArgs);
                var memberResult = parentTypeValue.GetMember(memberExp.MemberName, typeArgs.Length);

                switch (memberResult)
                {
                    case ItemQueryResult.Error.MultipleCandidates:
                        globalContext.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, memberExp);
                        break;

                    case ItemQueryResult.Error.VarWithTypeArg:
                        globalContext.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, memberExp);
                        break;

                    case ItemQueryResult.NotFound:
                        globalContext.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp);
                        break;

                    case ItemQueryResult.Type:
                        globalContext.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                        break;
                        
                    case ItemQueryResult.Funcs:
                        throw new NotImplementedException();

                    case ItemQueryResult.MemberVar memberVarResult:
                        // static인지 검사
                        if (memberVarResult.MemberVarInfo.IsStatic)
                            globalContext.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, memberExp);

                        var memberVarValue = globalContext.MakeMemberVarValue(memberVarResult.Outer, memberVarResult.MemberVarInfo);

                        var loc = MakeMemberLoc(parentTypeValue, parentLoc, memberExp.MemberName);
                        return new ExpResult.Exp(new R.LoadExp(loc), memberVarValue.GetTypeValue());
                }

                throw new UnreachableCodeException();
            }

            // T.x
            ExpResult AnalyzeMemberExpTypeParent(S.MemberExp nodeForErrorReport, TypeValue parentType, string memberName, ImmutableArray<S.TypeExp> stypeArgs)
            {   
                var member = parentType.GetMember(memberName, stypeArgs.Length);

                switch (member)
                {
                    case ItemQueryResult.NotFound:
                        globalContext.AddFatalError(A2007_ResolveIdentifier_NotFound, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    case ItemQueryResult.Error.MultipleCandidates:
                        globalContext.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    case ItemQueryResult.Error.VarWithTypeArg:
                        globalContext.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    case ItemQueryResult.Type typeResult:
                        {
                            var typeArgs = GetTypeValues(stypeArgs);
                            var typeValue = globalContext.MakeTypeValue(typeResult.Outer, typeResult.TypeInfo, typeArgs);
                            return new ExpResult.Type(typeValue);
                        }

                    case ItemQueryResult.Funcs funcsResult:
                        {
                            // 함수는 이 단계에서 타입인자가 확정되지 않으므로 재료들을 상위로 올려 보낸다
                            var typeArgs = GetTypeValues(stypeArgs);
                            return new ExpResult.Funcs(funcsResult.Outer, funcsResult.FuncInfos, typeArgs);
                        }

                    case ItemQueryResult.MemberVar memberVarResult:
                        var memberVarValue = globalContext.MakeMemberVarValue(memberVarResult.Outer, memberVarResult.MemberVarInfo);

                        if (!memberVarValue.IsStatic)
                        {
                            globalContext.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);
                            throw new UnreachableCodeException();
                        }

                        var rparentType = parentType.GetRPath();
                        var loc = new R.StaticMemberLoc(rparentType, memberName);
                        return new ExpResult.Loc(loc, memberVarValue.GetTypeValue());

                    default:
                        throw new UnreachableCodeException();
                }
            }

            void HandleErrorIdentifierResult(S.ISyntaxNode nodeForErrorReport, IdentifierResult.Error errorResult)
            {
                switch (errorResult)
                {
                    case IdentifierResult.Error.MultipleCandiates:
                        globalContext.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport);
                        break;

                    case IdentifierResult.Error.VarWithTypeArg:
                        globalContext.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport);
                        break;

                    case IdentifierResult.Error.CantGetStaticMemberThroughInstance:
                        globalContext.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, nodeForErrorReport);
                        break;

                    case IdentifierResult.Error.CantGetTypeMemberThroughInstance:
                        globalContext.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, nodeForErrorReport);
                        break;

                    case IdentifierResult.Error.CantGetInstanceMemberThroughType:
                        globalContext.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);
                        break;

                    case IdentifierResult.Error.FuncCantHaveMember:
                        globalContext.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, nodeForErrorReport);
                        break;
                }

                throw new UnreachableCodeException();
            }

            // exp를 돌려주는 버전
            // parent."x"<>
            ExpResult AnalyzeMemberExp(S.MemberExp memberExp)
            {
                var parentResult = AnalyzeExp(memberExp.Parent, ResolveHint.None);

                switch (parentResult)
                {
                    case ExpResult.Namespace:
                        throw new NotImplementedException();

                    case ExpResult.Type typeResult:
                        return AnalyzeMemberExpTypeParent(memberExp, typeResult.TypeValue, memberExp.MemberName, memberExp.MemberTypeArgs);

                    case ExpResult.EnumElem:
                        globalContext.AddFatalError(A2009_ResolveIdentifier_EnumElemCantHaveMember, memberExp);
                        break;

                    case ExpResult.Funcs:
                        globalContext.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp);
                        break;

                    case ExpResult.Exp expResult:
                        return AnalyzeMemberExpExpParent(memberExp, new R.TempLoc(expResult.Result, expResult.TypeValue.GetRPath()), expResult.TypeValue);

                    case ExpResult.Loc locResult:
                        return AnalyzeMemberExpExpParent(memberExp, locResult.Result, locResult.TypeValue);
                }

                throw new UnreachableCodeException();

                //var memberExpAnalyzer = new MemberExpAnalyzer(analyzer, memberExp);
                //var result = memberExpAnalyzer.Analyze();

                //if (result != null)
                //{
                //    context.AddNodeInfo(memberExp, result.Value.MemberExpInfo);
                //    outTypeValue = result.Value.TypeValue;
                //    return true;
                //}
                //else
                //{
                //    outTypeValue = null;
                //    return false;
                //}
            }

            ExpResult.Exp AnalyzeListExp(S.ListExp listExp)
            {
                var builder = ImmutableArray.CreateBuilder<R.Exp>();

                // TODO: 타입 힌트도 이용해야 할 것 같다
                TypeValue? curElemTypeValue = (listExp.ElemType != null) ? globalContext.GetTypeValueByTypeExp(listExp.ElemType) : null;

                foreach (var elem in listExp.Elems)
                {
                    var elemResult = AnalyzeExp_Exp(elem, ResolveHint.None);

                    builder.Add(elemResult.Result);

                    if (curElemTypeValue == null)
                    {
                        curElemTypeValue = elemResult.TypeValue;
                        continue;
                    }

                    if (!EqualityComparer<TypeValue>.Default.Equals(curElemTypeValue, elemResult.TypeValue))
                    {
                        // TODO: 둘의 공통 조상을 찾아야 하는지 결정을 못했다..
                        globalContext.AddFatalError(A1702_ListExp_MismatchBetweenElementTypes, elem);
                    }
                }

                if (curElemTypeValue == null)
                    globalContext.AddFatalError(A1701_ListExp_CantInferElementTypeWithEmptyElement, listExp);

                var rtype = curElemTypeValue.GetRPath();

                return new ExpResult.Exp(
                    new R.ListExp(rtype, builder.ToImmutable()),
                    globalContext.GetListType(curElemTypeValue));
            }

            ExpResult AnalyzeExp(S.Exp exp, ResolveHint hint)
            {
                switch (exp)
                {
                    case S.IdentifierExp idExp: return AnalyzeIdExp(idExp, hint);
                    case S.BoolLiteralExp boolExp: return AnalyzeBoolLiteralExp(boolExp);
                    case S.IntLiteralExp intExp: return AnalyzeIntLiteralExp(intExp);
                    case S.StringExp stringExp: return AnalyzeStringExp(stringExp);
                    case S.UnaryOpExp unaryOpExp: return AnalyzeUnaryOpExp(unaryOpExp);
                    case S.BinaryOpExp binaryOpExp: return AnalyzeBinaryOpExp(binaryOpExp);
                    case S.CallExp callExp: return AnalyzeCallExp(callExp, hint);
                    case S.LambdaExp lambdaExp: return AnalyzeLambdaExp(lambdaExp);
                    case S.IndexerExp indexerExp: return AnalyzeIndexerExp(indexerExp);
                    case S.MemberExp memberExp: return AnalyzeMemberExp(memberExp);
                    case S.ListExp listExp: return AnalyzeListExp(listExp);
                    default: throw new UnreachableCodeException();
                }
            }

            // 리턴값을 가능한한 Loc으로 맞춰준다
            ExpResult.Loc AnalyzeExp_Loc(S.Exp exp, ResolveHint hint)
            {
                var result = AnalyzeExp(exp, hint);
                switch (result)
                {
                    case ExpResult.Exp expResult:
                        return new ExpResult.Loc(new R.TempLoc(expResult.Result, expResult.TypeValue.GetRPath()), expResult.TypeValue);

                    case ExpResult.Loc locResult:
                        return locResult;
                }

                throw new UnreachableCodeException();
            }

            internal ExpResult.Exp AnalyzeExp_Exp(S.Exp exp, ResolveHint hint)
            {
                var result = AnalyzeExp(exp, hint);
                switch (result)
                {
                    case ExpResult.Exp expResult:
                        return expResult;

                    case ExpResult.Loc locResult:
                        return new ExpResult.Exp(new R.LoadExp(locResult.Result), locResult.TypeValue);
                }

                throw new UnreachableCodeException();
            }

            R.StringExpElement AnalyzeStringExpElement(S.StringExpElement elem)
            {
                if (elem is S.ExpStringExpElement expElem)
                {
                    var expResult = AnalyzeExp_Exp(expElem.Exp, ResolveHint.None);

                    // 캐스팅이 필요하다면 
                    if (globalContext.IsIntType(expResult.TypeValue))
                    {
                        return new R.ExpStringExpElement(
                            new R.CallInternalUnaryOperatorExp(
                                R.InternalUnaryOperator.ToString_Int_String,
                                expResult.Result
                            )
                        );
                    }
                    else if (globalContext.IsBoolType(expResult.TypeValue))
                    {
                        return new R.ExpStringExpElement(
                                new R.CallInternalUnaryOperatorExp(
                                R.InternalUnaryOperator.ToString_Bool_String,
                                expResult.Result
                            )
                        );
                    }
                    else if (globalContext.IsStringType(expResult.TypeValue))
                    {
                        return new R.ExpStringExpElement(expResult.Result);
                    }
                    else
                    {
                        globalContext.AddFatalError(A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, expElem.Exp);
                    }
                }
                else if (elem is S.TextStringExpElement textElem)
                {
                    return new R.TextStringExpElement(textElem.Text);
                }

                throw new UnreachableCodeException();
            }
        }
    }
}
