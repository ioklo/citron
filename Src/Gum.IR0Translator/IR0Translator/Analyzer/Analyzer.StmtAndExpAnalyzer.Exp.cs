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
                var result = IdExpIdentifierResolver.Resolve(idExp.Value, typeArgs, resolveHint, globalContext, callableContext, localContext);

                switch (result)
                {
                    case IdentifierResult.NotFound:
                        globalContext.AddFatalError(A2007_ResolveIdentifier_NotFound, idExp);
                        break;

                    case IdentifierResult.Error errorResult:
                        HandleErrorIdentifierResult(idExp, errorResult);
                        break;

                    case IdentifierResult.ThisVar:
                        var thisTypeValue = callableContext.GetThisTypeValue();
                        if (thisTypeValue == null)
                        {
                            globalContext.AddFatalError(A2010_ResolveIdentifier_ThisIsNotInTheContext, idExp);
                            break;
                        }
                        else
                        {
                            return new ExpResult.Loc(R.ThisLoc.Instance, thisTypeValue);
                        }

                    case IdentifierResult.LocalVarOutsideLambda localVarOutsideLambdaResult:
                        
                        // TODO: 여러번 캡쳐해도 한번만
                        callableContext.AddLambdaCapture(localVarOutsideLambdaResult.VarName, localVarOutsideLambdaResult.TypeValue);
                        return new ExpResult.Loc(new R.CapturedVarLoc(localVarOutsideLambdaResult.VarName), localVarOutsideLambdaResult.TypeValue);

                    case IdentifierResult.LocalVar localVarResult:
                        {
                            R.Loc loc = new R.LocalVarLoc(localVarResult.VarName);

                            if (localVarResult.IsRef)
                                loc = new R.DerefLocLoc(loc);

                            return new ExpResult.Loc(loc, localVarResult.TypeValue);
                        }

                    case IdentifierResult.GlobalVar globalVarResult:
                        {
                            R.Loc loc = new R.GlobalVarLoc(globalVarResult.VarName);

                            if (globalVarResult.IsRef)
                                loc = new R.DerefLocLoc(loc);

                            return new ExpResult.Loc(loc, globalVarResult.TypeValue);
                        }

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

                    case IdentifierResult.Type typeResult:
                        return new ExpResult.Type(typeResult.TypeValue);

                    case IdentifierResult.MemberVar memberVarResult:
                        {
                            var memberVarValue = globalContext.MakeMemberVarValue(memberVarResult.Outer, memberVarResult.MemberVarInfo);

                            if (memberVarValue.IsStatic)
                            {
                                return new ExpResult.Loc(new R.StaticMemberLoc(memberVarValue.GetRPath_Nested()), memberVarValue.GetTypeValue());
                            }
                            else
                            {
                                var loc = memberVarResult.Outer.MakeMemberLoc(R.ThisLoc.Instance, memberVarValue.GetRPath_Nested());
                                return new ExpResult.Loc(loc, memberVarValue.GetTypeValue());
                            }
                        }

                    case IdentifierResult.EnumElem enumElemResult:
                        return new ExpResult.EnumElem(enumElemResult.EnumElemTypeValue);
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
                    catch (AnalyzerFatalException)
                    {
                        bFatal = true;
                    }
                }

                if (bFatal)
                    throw new AnalyzerFatalException();

                return new ExpResult.Exp(new R.StringExp(builder.ToImmutable()), globalContext.GetStringType());
            }


            // int만 지원한다
            ExpResult.Exp AnalyzeIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
            {
                var operandResult = AnalyzeExp(operand, ResolveHint.None);

                if (operandResult is ExpResult.Loc locResult)
                {
                    // int type 검사, exact match
                    if (!locResult.TypeValue.Equals(globalContext.GetIntType()))
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
                            // exact match
                            if (!operandResult.TypeValue.Equals(globalContext.GetBoolType()))
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
                            if (!operandResult.TypeValue.Equals(globalContext.GetIntType()))
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
                    // 안되는거 체크
                    switch (destLocResult.Result)
                    {
                        // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
                        case R.CapturedVarLoc:
                            globalContext.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                            throw new UnreachableCodeException();

                        case R.ThisLoc:          // this = x;
                            globalContext.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                            throw new UnreachableCodeException();

                        case R.TempLoc:
                            throw new UnreachableCodeException();
                    }
                    
                    R.Loc destLoc = destLocResult.Result;
                    TypeValue destType = destLocResult.TypeValue;
                    
                    var operandHint1 = ResolveHint.Make(destType);
                    var srcResult = AnalyzeExp_Exp(exp.Operand1, operandHint1);
                    var wrappedSrcResult = CastExp_Exp(srcResult, destType, exp);

                    return new ExpResult.Exp(new R.AssignExp(destLoc, wrappedSrcResult.Result), destType);
                    
                }
                else
                {
                    globalContext.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                    throw new UnreachableCodeException();
                }
            }            

            ExpResult.Exp AnalyzeBinaryOpExp(S.BinaryOpExp binaryOpExp)
            {
                // 1. Assign 먼저 처리
                if (binaryOpExp.Kind == S.BinaryOpKind.Assign)
                {
                    return AnalyzeAssignBinaryOpExp(binaryOpExp);
                }

                var operandResult0 = AnalyzeExp_Exp(binaryOpExp.Operand0, ResolveHint.None);
                var operandResult1 = AnalyzeExp_Exp(binaryOpExp.Operand1, ResolveHint.None);

                // 2. NotEqual 처리
                if (binaryOpExp.Kind == S.BinaryOpKind.NotEqual)
                {
                    var equalInfos = globalContext.GetBinaryOpInfos(S.BinaryOpKind.Equal);
                    foreach (var info in equalInfos)
                    {
                        var castResult0 = globalContext.TryCastExp_Exp(operandResult0, info.OperandType0);
                        var castResult1 = globalContext.TryCastExp_Exp(operandResult1, info.OperandType1);

                        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                        if (castResult0 != null && castResult1 != null)
                        {
                            var equalExp = new R.CallInternalBinaryOperatorExp(
                                info.IR0Operator,
                                castResult0.Result,
                                castResult1.Result
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
                    var castResult0 = globalContext.TryCastExp_Exp(operandResult0, info.OperandType0);
                    var castResult1 = globalContext.TryCastExp_Exp(operandResult1, info.OperandType1);

                    // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                    if (castResult0 != null && castResult1 != null)
                    {
                        return new ExpResult.Exp(
                            new R.CallInternalBinaryOperatorExp(
                                info.IR0Operator,
                                castResult0.Result,
                                castResult1.Result),
                            info.ResultType);
                    }
                }

                // Operator를 찾을 수 없습니다
                globalContext.AddFatalError(A0802_BinaryOp_OperatorNotFound, binaryOpExp);
                return default; // suppress CS0161
            }

            ExpResult AnalyzeCallExpEnumElemCallable(EnumElemTypeValue elemTypeValue, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
            {
                if (elemTypeValue.IsStandalone())
                    globalContext.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, nodeForErrorReport);

                var fieldParamTypes = elemTypeValue.GetConstructorParamTypes();

                // 일단 lambda파라미터는 params를 지원하지 않는 것으로
                // args는 params를 지원 할 수 있음
                // TODO: MatchFunc에 OuterTypeEnv를 넣는 것이 나은지, fieldParamTypes에 미리 적용해서 넣는 것이 나은지
                // paramTypes으로 typeValues를 건네 줄것이면 적용해서 넣는게 나을 것 같은데, TypeResolver 동작때문에 어떻게 될지 몰라서 일단 여기서는 적용하고 TypeEnv.None을 넘겨준다
                var result = FuncMatcher.Match(globalContext, callableContext, localContext, TypeEnv.None, fieldParamTypes, null, default, sargs);

                if (result != null)
                {
                    return new ExpResult.Exp(
                        new R.NewEnumElemExp(elemTypeValue.GetRPath_Nested(), result.Value.Args),
                        elemTypeValue
                    );
                }
                else
                {
                    globalContext.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport);
                    throw new UnreachableCodeException();
                }
            }
            
            
            

            // CallExp분석에서 Callable이 Identifier인 경우 처리
            ExpResult AnalyzeCallExpFuncCallable(ExpResult.Funcs funcsResult, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
            {
                // 함수 중 하나를 골라야 한다
                // 1. 인자 개수가 맞는지 (params도 고려)
                // 2. 인자 타입이 맞는지 (타입 인퍼런스도 고려)
                // 3. 맞는개 여러개라면 1) (타입 인퍼런스 < 고정 인자) 우선, 2) 각각 (타입인퍼런스된 함수, 고정 인자 함수) 도 여러개라면 에러                

                // TypeEnv작성                
                var outerTypeEnv = funcsResult.Outer.GetTypeEnv();
                var matchedFunc = FuncMatcher.Match(globalContext, callableContext, localContext, outerTypeEnv, funcsResult.FuncInfos, sargs, funcsResult.TypeArgs, nodeForErrorReport);

                // funcValue만들기
                var funcValue = globalContext.MakeFuncValue(funcsResult.Outer, matchedFunc.CallableInfo, matchedFunc.TypeArgs);

                if (!funcValue.CheckAccess(callableContext.GetThisTypeValue()))
                    globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                if (funcValue.IsSequence)
                {
                    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
                    var seqTypeValue = globalContext.GetSeqTypeValue(funcValue.GetRPath_Nested(), funcValue.GetRetType());
                    return new ExpResult.Exp(new R.CallSeqFuncExp(funcValue.GetRPath_Nested(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
                }
                else
                {
                    // 만약
                    return new ExpResult.Exp(new R.CallFuncExp(funcValue.GetRPath_Nested(), funcsResult.Instance, matchedFunc.Args), funcValue.GetRetType());
                }
            }

            // CallExp 분석에서 Callable이 Exp인 경우 처리
            ExpResult.Exp AnalyzeCallExpExpCallable(R.Loc callableLoc, TypeValue callableTypeValue, ImmutableArray<S.Argument> sargs, S.CallExp nodeForErrorReport)
            {
                var lambdaType = callableTypeValue as LambdaTypeValue;
                if (lambdaType == null)
                    globalContext.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, nodeForErrorReport.Callable);

                // 일단 lambda파라미터는 params를 지원하지 않는 것으로
                // args는 params를 지원 할 수 있음

                var result = FuncMatcher.Match(globalContext, callableContext, localContext, TypeEnv.None, lambdaType.Params, null, default, sargs);

                if (result != null)
                {
                    return new ExpResult.Exp(
                        new R.CallValueExp(lambdaType.Lambda, callableLoc, result.Value.Args),
                        lambdaType.Return
                    );
                }
                else
                {
                    globalContext.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport);
                    throw new UnreachableCodeException();
                }
            }

            ExpResult.Exp AnalyzeCallExpTypeCallable(TypeValue callableTypeValue, ImmutableArray<S.Argument> sargs, S.CallExp nodeForErrorReport)
            {
                // NOTICE: 생성자 검색 (AnalyzeNewExp 부분과 비슷)
                if (callableTypeValue is StructTypeValue structTypeValue)
                {
                    // 생성자 검색,
                    var result = structTypeValue.GetMember(M.SpecialNames.Constructor, typeParamCount: 0); // NOTICE: constructor는 타입 파라미터가 없다
                    switch(result)
                    {
                        case ItemQueryResult.Constructors constructorResult:
                            var matchedConstructor = FuncMatcher.Match(globalContext, callableContext, localContext, structTypeValue.MakeTypeEnv(), constructorResult.ConstructorInfos, sargs, default, nodeForErrorReport);                            

                            var constructorValue = globalContext.MakeConstructorValue(constructorResult.Outer, matchedConstructor.CallableInfo);

                            if (!constructorValue.CheckAccess(callableContext.GetThisTypeValue()))
                                globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                            return new ExpResult.Exp(new R.NewStructExp(constructorValue.GetRPath_Nested(), matchedConstructor.Args), structTypeValue);

                        case ItemQueryResult.NotFound:
                            globalContext.AddFatalError(A0905_CallExp_NoConstructorFound, nodeForErrorReport);
                            break;

                        case ItemQueryResult.Error errorResult:
                            HandleItemQueryResultError(errorResult, nodeForErrorReport);
                            break;
                    }
                }

                globalContext.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, nodeForErrorReport);
                throw new UnreachableCodeException();
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

                    // callable이 타입으로 계산되면, Struct의 경우 생성자 호출을 한다
                    case ExpResult.Type typeResult:
                        return AnalyzeCallExpTypeCallable(typeResult.TypeValue, exp.Args, exp);

                    case ExpResult.Funcs funcsResult:
                        return AnalyzeCallExpFuncCallable(funcsResult, exp.Args, exp);

                    case ExpResult.Exp expResult:
                        var tempLoc = new R.TempLoc(expResult.Result, expResult.TypeValue.GetRPath());
                        return AnalyzeCallExpExpCallable(tempLoc, expResult.TypeValue, exp.Args, exp);

                    case ExpResult.Loc locResult:
                        return AnalyzeCallExpExpCallable(locResult.Result, locResult.TypeValue, exp.Args, exp);

                    // enum constructor, E.First
                    case ExpResult.EnumElem enumElemResult:                        
                        return AnalyzeCallExpEnumElemCallable(enumElemResult.EnumElemTypeValue, exp.Args, exp);
                }

                throw new UnreachableCodeException();
            }

            ExpResult.Exp AnalyzeLambdaExp(S.LambdaExp exp)
            {
                // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
                TypeValue? retTypeValue = null;

                var lambdaName = callableContext.NewAnonymousName();
                var lambdaPath = MakePath(lambdaName, R.ParamHash.None, default);
                var newLambdaContext = new LambdaContext(lambdaPath, callableContext.GetThisTypeValue(), localContext, retTypeValue);
                var newLocalContext = new LocalContext();
                var newAnalyzer = new StmtAndExpAnalyzer(globalContext, newLambdaContext, newLocalContext);

                // 파라미터는 람다 함수의 지역변수로 취급한다
                var paramInfosBuilder = ImmutableArray.CreateBuilder<ParamInfo>(exp.Params.Length);
                var rparamsBuilder = ImmutableArray.CreateBuilder<R.Param>(exp.Params.Length);
                foreach (var param in exp.Params)
                {
                    if (param.Type == null)
                        globalContext.AddFatalError(A9901_NotSupported_LambdaParameterInference, exp);

                    var rparamKind = param.ParamKind switch
                    {
                        S.FuncParamKind.Normal => R.ParamKind.Normal,
                        S.FuncParamKind.Params => R.ParamKind.Params,
                        S.FuncParamKind.Ref => R.ParamKind.Ref,
                        _ => throw new UnreachableCodeException()
                    };

                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    paramInfosBuilder.Add(new ParamInfo(rparamKind, paramTypeValue));
                    rparamsBuilder.Add(new R.Param(rparamKind, paramTypeValue.GetRPath(), param.Name));

                    // 람다 파라미터를 지역 변수로 추가한다
                    newLocalContext.AddLocalVarInfo(param.ParamKind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }   

                // 본문 분석
                var bodyResult = newAnalyzer.AnalyzeStmt(exp.Body);

                // 성공했으면, 리턴 타입 갱신            
                var capturedLocalVars = newLambdaContext.GetCapturedLocalVars();

                var paramTypes = paramInfosBuilder.MoveToImmutable();
                var rparamInfos = rparamsBuilder.MoveToImmutable(); // 이 이후로 rparamsBuilder스면 안됨

                // TODO: need capture this확인해서 this 넣기
                // var bCaptureThis = newLambdaContext.NeedCaptureThis();
                R.Path? capturedThisType = null;

                var capturedStmt = new R.CapturedStatement(capturedThisType, capturedLocalVars, bodyResult.Stmt);
                var lambdaDecl = new R.LambdaDecl(lambdaName, capturedStmt, rparamInfos);
                callableContext.AddDecl(lambdaDecl);

                var lambdaTypeValue = globalContext.GetLambdaTypeValue(
                    lambdaPath,
                    newLambdaContext.GetRetTypeValue() ?? globalContext.GetVoidType(),
                    paramTypes
                );

                return new ExpResult.Exp(
                    new R.LambdaExp(lambdaTypeValue.Lambda),
                    lambdaTypeValue);
            }
            
            ExpResult AnalyzeIndexerExp(S.IndexerExp exp)
            {
                var objResult = AnalyzeExp_Loc(exp.Object, ResolveHint.None);
                var indexResult = AnalyzeExp_Exp(exp.Index, ResolveHint.None);
                var castIndexResult = CastExp_Exp(indexResult, globalContext.GetIntType(), exp.Index);

                // TODO: custom indexer를 만들수 있으면 좋은가
                // var memberResult = objResult.TypeValue.GetMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

                // 리스트 타입의 경우,
                if (objResult.TypeValue is RuntimeListTypeValue listTypeValue)
                {
                    return new ExpResult.Loc(new R.ListIndexerLoc(objResult.Result, castIndexResult.Result), listTypeValue.ElemType);
                }
                else
                {
                    throw new NotImplementedException();
                }


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
            
            // exp.x
            ExpResult AnalyzeMemberExpLocParent(S.MemberExp memberExp, R.Loc parentLoc, TypeValue parentTypeValue)
            {
                var typeArgs = GetTypeValues(memberExp.MemberTypeArgs);
                var memberResult = parentTypeValue.GetMember(memberExp.MemberName, typeArgs.Length);

                switch (memberResult)
                {
                    case ItemQueryResult.Error errorResult:
                        HandleItemQueryResultError(errorResult, memberExp);
                        break;

                    case ItemQueryResult.NotFound:
                        globalContext.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp);
                        break;

                    case ItemQueryResult.Type:
                        globalContext.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                        break;

                    case ItemQueryResult.Constructors:
                        throw new UnreachableCodeException(); // 사용자는 명시적으로 생성자를 지칭할 수 없다

                    // x.First
                    case ItemQueryResult.EnumElem:
                        globalContext.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                        break;

                    case ItemQueryResult.Funcs funcsResult:

                        if (funcsResult.IsInstanceFunc)
                        {
                            return new ExpResult.Funcs(funcsResult.Outer, funcsResult.FuncInfos, typeArgs, parentLoc);
                        }
                        else
                        {
                            return new ExpResult.Funcs(funcsResult.Outer, funcsResult.FuncInfos, typeArgs, null);
                        }                            


                    case ItemQueryResult.MemberVar memberVarResult:                       

                        var memberVarValue = globalContext.MakeMemberVarValue(memberVarResult.Outer, memberVarResult.MemberVarInfo);

                        // static인지 검사
                        if (memberVarValue.IsStatic)                        
                            globalContext.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, memberExp);

                        // access modifier 검사
                        if (!memberVarValue.CheckAccess(callableContext.GetThisTypeValue()))
                            globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, memberExp);

                        var memberLoc = parentTypeValue.MakeMemberLoc(parentLoc, memberVarValue.GetRPath_Nested());
                        return new ExpResult.Loc(memberLoc, memberVarValue.GetTypeValue());
                }

                throw new UnreachableCodeException();
            }

            void HandleItemQueryResultError(ItemQueryResult.Error error, S.ISyntaxNode nodeForErrorReport)
            {
                switch (error)
                {
                    case ItemQueryResult.Error.MultipleCandidates:
                        globalContext.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    case ItemQueryResult.Error.VarWithTypeArg:
                        globalContext.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    default:
                        throw new UnreachableCodeException();
                }
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

                    case ItemQueryResult.Error errorResult:
                        HandleItemQueryResultError(errorResult, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    case ItemQueryResult.Type typeResult:
                        {
                            var typeArgs = GetTypeValues(stypeArgs);
                            var typeValue = globalContext.MakeTypeValue(typeResult.Outer, typeResult.TypeInfo, typeArgs);
                            return new ExpResult.Type(typeValue);
                        }

                    case ItemQueryResult.Constructors:
                        throw new UnreachableCodeException(); // 사용자는 명시적으로 생성자를 지칭할 수 없다

                    // T.F가 나왔다. static이어야 한다
                    case ItemQueryResult.Funcs funcsResult:
                        {
                            // 함수는 이 단계에서 타입인자가 확정되지 않으므로 재료들을 상위로 올려 보낸다
                            var typeArgs = GetTypeValues(stypeArgs);

                            if (funcsResult.IsInstanceFunc)
                            {
                                globalContext.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);
                                throw new UnreachableCodeException();
                            }

                            return new ExpResult.Funcs(funcsResult.Outer, funcsResult.FuncInfos, typeArgs, null);
                        }

                    case ItemQueryResult.MemberVar memberVarResult:
                        var memberVarValue = globalContext.MakeMemberVarValue(memberVarResult.Outer, memberVarResult.MemberVarInfo);

                        if (!memberVarValue.IsStatic)
                            globalContext.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);

                        if (!memberVarValue.CheckAccess(callableContext.GetThisTypeValue()))
                            globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);
                        
                        var loc = new R.StaticMemberLoc(memberVarValue.GetRPath_Nested());
                        return new ExpResult.Loc(loc, memberVarValue.GetTypeValue());

                    // E.First
                    case ItemQueryResult.EnumElem enumElemResult:
                        var elemTypeValue = globalContext.MakeEnumElemTypeValue(enumElemResult.Outer, enumElemResult.EnumElemInfo);
                        return new ExpResult.EnumElem(elemTypeValue);

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
                if (memberExp.Parent is S.IdentifierExp idParent)
                {
                    var typeArgs = GetTypeValues(idParent.TypeArgs);
                    var result = IdExpIdentifierResolver.Resolve(idParent.Value, typeArgs, ResolveHint.None, globalContext, callableContext, localContext);

                    switch (result)
                    {
                        case IdentifierResult.NotFound:
                            globalContext.AddFatalError(A2007_ResolveIdentifier_NotFound, idParent);
                            throw new UnreachableCodeException();

                        case IdentifierResult.Error errorResult:
                            HandleErrorIdentifierResult(idParent, errorResult);
                            throw new UnreachableCodeException();

                        case IdentifierResult.ThisVar:
                            {
                                var thisTypeValue = callableContext.GetThisTypeValue();
                                if (thisTypeValue == null)
                                {
                                    globalContext.AddFatalError(A2010_ResolveIdentifier_ThisIsNotInTheContext, idParent);
                                    throw new UnreachableCodeException();
                                }

                                return AnalyzeMemberExpLocParent(memberExp, R.ThisLoc.Instance, thisTypeValue);
                            }

                        case IdentifierResult.LocalVarOutsideLambda localVarOutsideLambdaResult:
                            // TODO: 여러번 캡쳐해도 한번만
                            callableContext.AddLambdaCapture(localVarOutsideLambdaResult.VarName, localVarOutsideLambdaResult.TypeValue);
                            return AnalyzeMemberExpLocParent(memberExp, new R.CapturedVarLoc(localVarOutsideLambdaResult.VarName), localVarOutsideLambdaResult.TypeValue);

                        case IdentifierResult.LocalVar localVarResult:
                            {
                                R.Loc loc = new R.LocalVarLoc(localVarResult.VarName);
                                if (localVarResult.IsRef)
                                    loc = new R.DerefLocLoc(loc);

                                return AnalyzeMemberExpLocParent(memberExp, loc, localVarResult.TypeValue);
                            }

                        case IdentifierResult.GlobalVar globalVarResult:
                            {
                                R.Loc loc = new R.GlobalVarLoc(globalVarResult.VarName);
                                if (globalVarResult.IsRef)
                                    loc = new R.DerefLocLoc(loc);

                                return AnalyzeMemberExpLocParent(memberExp, loc, globalVarResult.TypeValue);
                            }

                        // F.x 는
                        case IdentifierResult.Funcs:
                            globalContext.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp);
                            throw new UnreachableCodeException();                            
                        
                        case IdentifierResult.Type typeResult:
                            return AnalyzeMemberExpTypeParent(memberExp, typeResult.TypeValue, memberExp.MemberName, memberExp.MemberTypeArgs);
                        
                        case IdentifierResult.EnumElem:
                            // ResolveHint.None 이었기 때문에 나올 수 없다
                            throw new UnreachableCodeException();

                        default:
                            throw new UnreachableCodeException();
                    }
                }
                else
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
                            return AnalyzeMemberExpLocParent(memberExp, new R.TempLoc(expResult.Result, expResult.TypeValue.GetRPath()), expResult.TypeValue);

                        case ExpResult.Loc locResult:
                            return AnalyzeMemberExpLocParent(memberExp, locResult.Result, locResult.TypeValue);
                    }

                    throw new UnreachableCodeException();
                }


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

            ExpResult.Exp AnalyzeNewExp(S.NewExp newExp)
            {
                var classTypeValue = globalContext.GetTypeValueByTypeExp(newExp.Type) as ClassTypeValue;
                if (classTypeValue == null)
                    globalContext.AddFatalError(A2601_NewExp_TypeIsNotClass, newExp.Type);

                // NOTICE: 생성자 검색 (AnalyzeCallExpTypeCallable 부분과 비슷)
                var result = classTypeValue.GetMember(M.SpecialNames.Constructor, typeParamCount: 0); // NOTICE: constructor는 타입 파라미터가 없다
                switch (result)
                {
                    case ItemQueryResult.Constructors constructorResult:
                        var matchedConstructor = FuncMatcher.Match(globalContext, callableContext, localContext, classTypeValue.MakeTypeEnv(), constructorResult.ConstructorInfos, newExp.Args, default, newExp);

                        var constructorValue = globalContext.MakeConstructorValue(constructorResult.Outer, matchedConstructor.CallableInfo);

                        if (!constructorValue.CheckAccess(callableContext.GetThisTypeValue()))
                            globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, newExp);

                        return new ExpResult.Exp(
                            new R.NewClassExp(
                                classTypeValue.GetRPath_Nested(), 
                                constructorValue.GetRPath_Nested().ParamHash, 
                                matchedConstructor.Args
                            ), 
                            classTypeValue);

                    case ItemQueryResult.NotFound:
                        globalContext.AddFatalError(A2602_NewExp_NoConstructorFound, newExp);
                        break;

                    case ItemQueryResult.Error errorResult:
                        HandleItemQueryResultError(errorResult, newExp);
                        break;
                }
                
                throw new UnreachableCodeException();
            }

            public ExpResult AnalyzeExp(S.Exp exp, ResolveHint hint)
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
                    case S.NewExp newExp: return AnalyzeNewExp(newExp);
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

            public static ExpResult.Exp AnalyzeExp_Exp(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext, S.Exp exp, ResolveHint hint)
            {
                var analyzer = new StmtAndExpAnalyzer(globalContext, callableContext, localContext);
                return analyzer.AnalyzeExp_Exp(exp, hint);
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

                    case ExpResult.Type typeResult:
                        if (typeResult.TypeValue is EnumElemTypeValue enumElemTypeValue) // E.F
                        {
                            if (enumElemTypeValue.IsStandalone())
                                return new ExpResult.Exp(new R.NewEnumElemExp(enumElemTypeValue.GetRPath_Nested(), default), enumElemTypeValue);
                            else // 함수로 
                                throw new NotImplementedException();
                        }
                        globalContext.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, exp);
                        throw new UnreachableCodeException();

                    case ExpResult.EnumElem enumElemResult:
                        if (enumElemResult.EnumElemTypeValue.IsStandalone())
                            return new ExpResult.Exp(new R.NewEnumElemExp(enumElemResult.EnumElemTypeValue.GetRPath_Nested(), default), enumElemResult.EnumElemTypeValue);
                        else
                            throw new NotImplementedException(); // 함수로

                    default:
                        throw new UnreachableCodeException();
                }
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

            // 값의 겉보기 타입을 변경한다
            internal ExpResult.Exp CastExp_Exp(ExpResult.Exp expResult, TypeValue expectType, S.ISyntaxNode nodeForErrorReport) // throws AnalyzeFatalException
            {
                var result = globalContext.TryCastExp_Exp(expResult, expectType);
                if (result != null) return result;

                globalContext.AddFatalError(A2201_Cast_Failed, nodeForErrorReport);
                throw new UnreachableCodeException();
            }
        }
    }
}
