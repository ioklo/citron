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
using Gum.Analysis;

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
                        var thisTypeValue = callableContext.GetThisType();
                        if (thisTypeValue == null)
                        {
                            globalContext.AddFatalError(A2010_ResolveIdentifier_ThisIsNotInTheContext, idExp);
                            break;
                        }
                        else
                        {
                            return new ExpResult.Loc(new R.ThisLoc(), thisTypeValue);
                        }

                    case IdentifierResult.LocalVar localVarResult:
                        {
                            R.Loc loc = new R.LocalVarLoc(new R.Name.Normal(localVarResult.VarName));

                            if (localVarResult.IsRef)
                                loc = new R.DerefLocLoc(loc);

                            return new ExpResult.Loc(loc, localVarResult.TypeSymbol);
                        }

                    case IdentifierResult.LocalVarOutsideLambda localVarOutsideLambdaResult:
                        
                        // TODO: 여러번 캡쳐해도 한번만
                        var memberVar = callableContext.AddLambdaCapture(localVarOutsideLambdaResult.VarName, localVarOutsideLambdaResult.TypeSymbol);
                        return new ExpResult.Loc(new R.LambdaMemberVarLoc(memberVar), localVarOutsideLambdaResult.TypeSymbol);

                    case IdentifierResult.GlobalVar globalVarResult:
                        {
                            R.Loc loc = new R.GlobalVarLoc(globalVarResult.VarName);

                            if (globalVarResult.IsRef)
                                loc = new R.DerefLocLoc(loc);

                            return new ExpResult.Loc(loc, globalVarResult.TypeSymbol);
                        }

                    case IdentifierResult.GlobalFuncs globalFuncsResult:
                        return new ExpResult.GlobalFuncs(globalFuncsResult.QueryResult, globalFuncsResult.TypeArgsForMatch);

                    // 'C'
                    case IdentifierResult.Class classResult:
                        return new ExpResult.Class(classResult.Symbol);

                    // 'F' (inside class context C)
                    case IdentifierResult.ClassMemberFuncs classMemberFuncsResult:                        
                        return new ExpResult.ClassMemberFuncs(classMemberFuncsResult.QueryResult, classMemberFuncsResult.TypeArgsForMatch, new R.ThisLoc(), bCheckInstanceForStatic: false);

                    // 'x' (inside class context C)
                    case IdentifierResult.ClassMemberVar classMemberVarResult:
                        {
                            var instance = classMemberVarResult.Symbol.IsStatic() ? null : new R.ThisLoc();
                            return new ExpResult.Loc(new R.ClassMemberLoc(instance, classMemberVarResult.Symbol), classMemberVarResult.Symbol.GetDeclType());
                        }

                    // 'S'
                    case IdentifierResult.Struct structResult:
                        return new ExpResult.Struct(structResult.Symbol);

                    // 'F' (inside struct context S)
                    case IdentifierResult.StructMemberFuncs structMemberFuncsResult:
                        return new ExpResult.StructMemberFuncs(structMemberFuncsResult.QueryResult, structMemberFuncsResult.TypeArgsForMatch, new R.ThisLoc(), bCheckInstanceForStatic: false);

                    // 'x' (inside struct context S)
                    case IdentifierResult.StructMemberVar structMemberVarResult:
                        {
                            var instance = structMemberVarResult.Symbol.IsStatic() ? null : new R.ThisLoc();
                            return new ExpResult.Loc(new R.StructMemberLoc(instance, structMemberVarResult.Symbol), structMemberVarResult.Symbol.GetDeclType());
                        }

                    // 'E'
                    case IdentifierResult.Enum enumResult:
                        return new ExpResult.Enum(enumResult.Symbol);

                    // 'First' (with type hint enum E)
                    case IdentifierResult.EnumElem enumElemResult:
                        return new ExpResult.EnumElem(enumElemResult.EnumElemSymbol);
                }

                throw new UnreachableCodeException();
            }

            ExpResult.Exp AnalyzeNullLiteralExp(S.NullLiteralExp nullExp, ResolveHint hint)
            {
                if (hint.TypeHint is TypeValueTypeHint typeValueHint)
                {
                    if (typeValueHint.TypeSymbol is NullableSymbol nullableTypeValue)
                    {
                        return new ExpResult.Exp(new R.NewNullableExp(null, nullableTypeValue));
                    }
                }

                globalContext.AddFatalError(A2701_NullLiteralExp_CantInferNullableType, nullExp);
                throw new UnreachableCodeException();
            }

            ExpResult.Exp AnalyzeBoolLiteralExp(S.BoolLiteralExp boolExp)
            {
                return new ExpResult.Exp(new R.BoolLiteralExp(boolExp.Value, globalContext.GetBoolType()));
            }

            ExpResult.Exp AnalyzeIntLiteralExp(S.IntLiteralExp intExp)
            {
                return new ExpResult.Exp(new R.IntLiteralExp(intExp.Value, globalContext.GetIntType()));
            }

            [AutoConstructor]
            partial struct StringExpResult
            {
                public R.StringExp Exp { get; }
                public ITypeSymbol TypeSymbol { get; }
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

                return new ExpResult.Exp(new R.StringExp(builder.ToImmutable(), globalContext.GetStringType()));
            }


            // int만 지원한다
            ExpResult.Exp AnalyzeIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
            {
                var operandResult = AnalyzeExp(operand, ResolveHint.None);

                if (operandResult is ExpResult.Loc locResult)
                {
                    // int type 검사, exact match
                    if (!globalContext.IsInt(locResult.TypeSymbol))
                        globalContext.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);

                    return new ExpResult.Exp(new R.CallInternalUnaryAssignOperatorExp(op, locResult.Result, intType));
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
                            if (!operandResult.Result.GetTypeSymbol().Equals(globalContext.GetBoolType()))
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
                            if (!globalContext.IsInt(operandResult.GetTypeSymbol()))
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

            void UpdateNullableState()
            {

            }

            ExpResult.Exp AnalyzeAssignBinaryOpExp(S.BinaryOpExp exp)
            {
                // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
                var destResult = AnalyzeExp(exp.Operand0, ResolveHint.None);

                if (destResult is ExpResult.Loc destLocResult)
                {
                    // 안되는거 체크
                    R.Loc destLoc = destLocResult.Result;
                    switch (destLoc)
                    {
                        // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
                        case R.LambdaMemberVarLoc:
                            globalContext.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                            throw new UnreachableCodeException();

                        case R.ThisLoc:          // this = x;
                            globalContext.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                            throw new UnreachableCodeException();

                        case R.TempLoc:
                            throw new UnreachableCodeException();
                    }                    
                    
                    ITypeSymbol destType = destLocResult.TypeSymbol;
                    
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


            (TFuncSymbol Func, ImmutableArray<R.Argument> Args) InternalMatchFunc<TFuncDeclSymbol, TFuncSymbol>(
                ImmutableArray<DeclAndConstructor<TFuncDeclSymbol, TFuncSymbol>> declAndConstructors,
                ImmutableArray<ITypeSymbol> typeArgsForMatch,
                ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
                where TFuncDeclSymbol : IFuncDeclSymbol
            {
                // TODO: 메모리 소비 제거
                var decls = ImmutableArray.CreateRange(declAndConstructors, declAndConstructor => declAndConstructor.GetDecl());

                // outer가 없으므로 outerTypeEnv는 None이다
                var result = FuncMatcher.MatchIndex(globalContext, callableContext, localContext, TypeEnv.Empty, decls, sargs, typeArgsForMatch);

                switch (result)
                {
                    case FuncMatchIndexResult.MultipleCandidates:
                        globalContext.AddFatalError(A0901_CallExp_MultipleCandidates, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    case FuncMatchIndexResult.NotFound:
                        globalContext.AddFatalError(A0906_CallExp_NotFound, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    case FuncMatchIndexResult.Success successResult:
                        var func = declAndConstructors[successResult.Index].MakeSymbol(successResult.TypeArgs);
                        return (func, successResult.Args);

                    default:
                        throw new UnreachableCodeException();
                }
            }

            // CallExp분석에서 Callable이 GlobalMemberFuncs인 경우 처리
            ExpResult.Exp AnalyzeCallExpGlobalFuncsCallable(ExpResult.GlobalFuncs funcs, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
            {
                var (func, args) = InternalMatchFunc(funcs.QueryResult.Infos, funcs.TypeArgsForMatch, sargs, nodeForErrorReport);

                if (!CanAccess(func))
                    globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);
                
                return new ExpResult.Exp(new R.CallGlobalFuncExp(func, args));
            }

            bool CanAccess(ISymbolNode target)
            {
                var thisNode = callableContext.GetThisNode();
                return thisNode.CanAccess(target);
            }

            // CallExp분석에서 Callable이 ClassMemberFuncs인 경우 처리
            ExpResult.Exp AnalyzeCallExpClassMemberFuncsCallable(ExpResult.ClassMemberFuncs funcs, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
            {
                var (func, args) = InternalMatchFunc(funcs.QueryResult.Infos, funcs.TypeArgsForMatch, sargs, nodeForErrorReport);

                if (CanAccess(func))
                    globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                // static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
                if (funcs.bCheckInstanceForStatic)
                {
                    // static this 체크
                    if (func.IsStatic() && funcs.Instance != null)
                        globalContext.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, nodeForErrorReport);

                    // 반대의 경우도 체크
                    if (!func.IsStatic() && funcs.Instance == null)
                        globalContext.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);
                }

                //if (func.IsSequence)
                //{
                //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
                //    var seqTypeValue = globalContext.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
                //    return new ExpResult.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
                //}
                //else
                {
                    return new ExpResult.Exp(new R.CallClassMemberFuncExp(func, funcs.Instance, args));
                }
            }

            // CallExp분석에서 Callable이 StructMemberFuncs인 경우 처리
            ExpResult.Exp AnalyzeCallExpStructMemberFuncsCallable(ExpResult.StructMemberFuncs funcs, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
            {
                var (func, args) = InternalMatchFunc(funcs.QueryResult.Infos, funcs.TypeArgsForMatch, sargs, nodeForErrorReport);

                if (CanAccess(func))
                    globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                // static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
                if (funcs.bCheckInstanceForStatic)
                {
                    // static this 체크
                    if (func.IsStatic() && funcs.Instance != null)
                        globalContext.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, nodeForErrorReport);

                    // 반대의 경우도 체크
                    if (!func.IsStatic() && funcs.Instance == null)
                        globalContext.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);
                }

                //if (func.IsSequence)
                //{
                //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
                //    var seqTypeValue = globalContext.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
                //    return new ExpResult.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
                //}
                //else
                {
                    return new ExpResult.Exp(new R.CallStructMemberFuncExp(func, funcs.Instance, args));
                }
            }

            // CallExp 분석에서 Callable이 Exp인 경우 처리
            ExpResult.Exp AnalyzeCallExpExpCallable(R.Loc callableLoc, ITypeSymbol callableTypeValue, ImmutableArray<S.Argument> sargs, S.CallExp nodeForErrorReport)
            {
                var lambdaType = callableTypeValue as LambdaSymbol;
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

            ExpResult.Exp AnalyzeCallExpEnumElemCallable(EnumElemSymbol elemTypeValue, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
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
                        new R.NewEnumElemExp(elemTypeValue.MakeRPath(), result.Value.Args),
                        elemTypeValue
                    );
                }
                else
                {
                    globalContext.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport);
                    throw new UnreachableCodeException();
                }
            }

            ExpResult.Exp AnalyzeCallExpStructCallable(StructSymbol structCallable, ImmutableArray<S.Argument> sargs, S.CallExp nodeForErrorReport)
            {
                // 생성자 검색,
                var structDecl = structCallable.GetDecl();

                var constructorDecls = ImmutableArray.CreateRange(structDecl.GetConstructorCount, structDecl.GetConstructor);

                var result = FuncMatcher.MatchIndex(globalContext, callableContext, localContext, 
                    structCallable.GetTypeEnv(), constructorDecls, sargs, default);

                switch (result)
                {
                    case FuncMatchIndexResult.MultipleCandidates:
                        globalContext.AddFatalError(A0907_CallExp_MultipleMatchedStructConstructors, nodeForErrorReport);
                        break;

                    case FuncMatchIndexResult.NotFound:
                        globalContext.AddFatalError(A0905_CallExp_NoMatchedStructConstructorFound, nodeForErrorReport);
                        break;

                    case FuncMatchIndexResult.Success successResult:
                        // 지금은 constructor가 typeArgs를 받지 않아서 structCallable에서 곧바로 가져올 수 있지만,
                        // TypeArgs가 추가된다면 Constructor도 Instantiate 해야 하고, StructSymbol.GetConstructor를 제거해야한다                        
                        // var constructorDecl = structDecl.GetConstructor(successResult.Index)
                        // SymbolInstantiator.Instantiate(factory, structCallable, constructorDecl, successResult.TypeArgs);
                        var constructor = structCallable.GetConstructor(successResult.Index);

                        if (!CanAccess(constructor))
                            globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                        return new ExpResult.Exp(new R.NewStructExp(constructor, successResult.Args));
                }

                throw new UnreachableCodeException();
            }

            ExpResult.Exp AnalyzeCallExpTypeCallable(ITypeSymbol callable, ImmutableArray<S.Argument> sargs, S.CallExp nodeForErrorReport)
            {
                switch(callable)
                {
                    case StructSymbol structCallable:
                        return AnalyzeCallExpStructCallable(structCallable, sargs, nodeForErrorReport);

                    // enum constructor, E.First
                    case EnumElemSymbol enumElemCallable:
                        return AnalyzeCallExpEnumElemCallable(enumElemCallable, sargs, nodeForErrorReport);
                }

                // NOTICE: 생성자 검색 (AnalyzeNewExp 부분과 비슷)
                if (callable is StructSymbol structTypeValue)
                {
                    
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
                if (hint.TypeHint is TypeValueTypeHint typeValueTypeHint && typeValueTypeHint.TypeSymbol is EnumSymbol enumSymbol)
                    callableHint = new ResolveHint(new EnumConstructorTypeHint(enumSymbol));
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

                    // callable이 타입으로 계산되면, Struct과 EnumElem의 경우 생성자 호출을 한다
                    case ExpResult.Type typeResult:
                        return AnalyzeCallExpTypeCallable(typeResult.TypeSymbol, exp.Args, exp);

                    case ExpResult.GlobalFuncs globalFuncsResult:
                        return AnalyzeCallExpGlobalFuncsCallable(globalFuncsResult, exp.Args, exp);

                    case ExpResult.ClassMemberFuncs classMemberfuncsResult:
                        return AnalyzeCallExpClassMemberFuncsCallable(classMemberfuncsResult, exp.Args, exp);

                    case ExpResult.StructMemberFuncs structMemberFuncsResult:
                        return AnalyzeCallExpStructMemberFuncsCallable(structMemberFuncsResult, exp.Args, exp);

                    case ExpResult.Exp expResult:
                        var tempLoc = new R.TempLoc(expResult.Result);
                        return AnalyzeCallExpExpCallable(tempLoc, expResult.Result.GetTypeSymbol(), exp.Args, exp);

                    case ExpResult.Loc locResult:
                        return AnalyzeCallExpExpCallable(locResult.Result, locResult.TypeSymbol, exp.Args, exp);
                }

                throw new UnreachableCodeException();
            }

            ExpResult.Exp AnalyzeLambdaExp(S.LambdaExp exp)
            {
                // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
                ITypeSymbol? retTypeValue = null;

                var lambdaName = callableContext.NewAnonymousName();
                var lambdaPath = MakePath(lambdaName, R.ParamHash.None, default);
                var newLambdaContext = new LambdaContext(lambdaPath, callableContext.GetThisType(), localContext, retTypeValue);
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
                        S.FuncParamKind.Normal => R.ParamKind.Default,
                        S.FuncParamKind.Params => R.ParamKind.Params,
                        S.FuncParamKind.Ref => R.ParamKind.Ref,
                        _ => throw new UnreachableCodeException()
                    };

                    var paramTypeValue = globalContext.GetSymbolByTypeExp(param.Type);
                    paramInfosBuilder.Add(new ParamInfo(rparamKind, paramTypeValue));
                    rparamsBuilder.Add(new R.Param(rparamKind, paramTypeValue.MakeRPath(), new R.Name.Normal(param.Name)));

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
                callableContext.AddCallableMemberDecl(lambdaDecl);

                var lambdaTypeValue = globalContext.MakeLambdaSymbol(
                    lambdaPath,
                    newLambdaContext.GetReturn() ?? globalContext.GetVoidType(),
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
                // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

                // 리스트 타입의 경우,
                if (objResult.TypeSymbol is RuntimeListTypeSymbol listTypeValue)
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
            ExpResult AnalyzeMemberExpLocParent(S.MemberExp memberExp, R.Loc parentLoc, ITypeSymbol instanceType)
            {
                var typeArgs = GetTypeValues(memberExp.MemberTypeArgs);
                var memberResult = instanceType.QueryMember(new M.Name.Normal(memberExp.MemberName), typeArgs.Length);

                switch (memberResult)
                {
                    case SymbolQueryResult.Error errorResult:
                        HandleItemQueryResultError(globalContext, errorResult, memberExp);
                        break;

                    case SymbolQueryResult.NotFound:
                        globalContext.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp);
                        break;

                    case SymbolQueryResult.GlobalFuncs:                        
                        throw new UnreachableCodeException(); // 나올 수가 없다

                    // exp.C
                    case SymbolQueryResult.Class:
                        globalContext.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                        break;

                    // exp.F
                    case SymbolQueryResult.ClassMemberFuncs classMemberFuncsResult:
                        return new ExpResult.ClassMemberFuncs(classMemberFuncsResult, typeArgs, parentLoc, bCheckInstanceForStatic: true);

                    // exp.x
                    case SymbolQueryResult.ClassMemberVar classMemberVarResult:
                        {
                            var symbol = classMemberVarResult.Var;

                            // static인지 검사
                            if (symbol.IsStatic())
                                globalContext.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, memberExp);

                            // access modifier 검사                            
                            if (!CanAccess(symbol))
                                globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, memberExp);

                            return new ExpResult.Loc(new R.ClassMemberLoc(parentLoc, symbol), symbol.GetDeclType());
                        }

                    // exp.S
                    case SymbolQueryResult.Struct:
                        globalContext.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                        break;

                    // exp.F
                    case SymbolQueryResult.StructMemberFuncs structMemberFuncsResult:
                        return new ExpResult.StructMemberFuncs(structMemberFuncsResult, typeArgs, parentLoc, bCheckInstanceForStatic: true);

                    // exp.x
                    case SymbolQueryResult.StructMemberVar structMemberVarResult:
                        {
                            var symbol = structMemberVarResult.Var;

                            // static인지 검사
                            if (symbol.IsStatic())
                                globalContext.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, memberExp);

                            // access modifier 검사                            
                            if (!CanAccess(symbol))
                                globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, memberExp);

                            return new ExpResult.Loc(new R.StructMemberLoc(parentLoc, symbol), symbol.GetDeclType());
                        }

                    // exp.E
                    case SymbolQueryResult.Enum:
                        globalContext.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                        break;

                    // E exp;
                    // exp.First
                    case SymbolQueryResult.EnumElem:
                        globalContext.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                        break;

                    // E.Second exp;
                    // exp.f
                    case SymbolQueryResult.EnumElemMemberVar enumElemMemberVarResult:
                        return new ExpResult.Loc(new R.EnumElemMemberLoc(parentLoc, enumElemMemberVarResult.Symbol), enumElemMemberVarResult.Symbol.GetDeclType());
                }

                throw new UnreachableCodeException();
            }
            
            // T.x
            ExpResult AnalyzeMemberExpTypeParent(S.MemberExp nodeForErrorReport, ITypeSymbol parentType, string memberName, ImmutableArray<S.TypeExp> stypeArgs)
            {   
                var member = parentType.QueryMember(new M.Name.Normal(memberName), stypeArgs.Length);

                switch (member)
                {
                    case SymbolQueryResult.NotFound:
                        globalContext.AddFatalError(A2007_ResolveIdentifier_NotFound, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    case SymbolQueryResult.Error errorResult:
                        HandleItemQueryResultError(globalContext, errorResult, nodeForErrorReport);
                        throw new UnreachableCodeException();

                    // T.C
                    case SymbolQueryResult.Class classResult:
                        {
                            var typeArgs = GetTypeValues(stypeArgs);                            
                            var classSymbol = classResult.ClassConstructor.Invoke(typeArgs);

                            // check access
                            if (!CanAccess(classSymbol))
                                globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                            return new ExpResult.Class(classSymbol);
                        }

                    // T.F
                    case SymbolQueryResult.ClassMemberFuncs classMemberFuncsResult:
                        {
                            var typeArgs = GetTypeValues(stypeArgs);
                            return new ExpResult.ClassMemberFuncs(classMemberFuncsResult, typeArgs, null, true);
                        }

                    case SymbolQueryResult.ClassMemberVar classMemberVarResult:
                        {
                            var symbol = classMemberVarResult.Var;

                            if (!symbol.IsStatic())
                                globalContext.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);

                            return new ExpResult.Loc(new R.ClassMemberLoc(null, symbol), symbol.GetDeclType());

                            var memberVarValue = globalContext.MakeMemberVarValue(memberVarResult.Outer, memberVarResult.MemberVarInfo);

                            if (!memberVarValue.IsStatic)
                                ;

                            if (!CanAccess(memberVarValue))
                                globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                            var loc = new R.StaticMemberLoc(memberVarValue.MakeRPath());
                            return new ExpResult.Loc(loc, memberVarValue.GetTypeValue());
                        }


                    case SymbolQueryResult.Type typeResult:
                        {
                            var typeArgs = GetTypeValues(stypeArgs);
                            var typeValue = globalContext.MakeTypeValue(typeResult.Outer, typeResult.TypeInfo, typeArgs);
                            return new ExpResult.Type(typeValue);
                        }

                    case SymbolQueryResult.Constructors:
                        throw new UnreachableCodeException(); // 사용자는 명시적으로 생성자를 지칭할 수 없다

                    // T.F가 나왔다. static이어야 한다
                    case SymbolQueryResult.Funcs funcsResult:
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

                    case SymbolQueryResult.MemberVar memberVarResult:
                        var memberVarValue = globalContext.MakeMemberVarValue(memberVarResult.Outer, memberVarResult.MemberVarInfo);

                        if (!memberVarValue.IsStatic)
                            globalContext.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);

                        if (!CanAccess(memberVarValue))
                            globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);
                        
                        var loc = new R.StaticMemberLoc(memberVarValue.MakeRPath());
                        return new ExpResult.Loc(loc, memberVarValue.GetTypeValue());

                    // E.First
                    case SymbolQueryResult.EnumElem enumElemResult:
                        return new ExpResult.EnumElem(enumElemResult.Symbol);

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
                                var thisTypeValue = callableContext.GetThisType();
                                if (thisTypeValue == null)
                                {
                                    globalContext.AddFatalError(A2010_ResolveIdentifier_ThisIsNotInTheContext, idParent);
                                    throw new UnreachableCodeException();
                                }

                                return AnalyzeMemberExpLocParent(memberExp, R.ThisLoc.Instance, thisTypeValue);
                            }

                        case IdentifierResult.LocalVarOutsideLambda localVarOutsideLambdaResult:
                            // TODO: 여러번 캡쳐해도 한번만
                            callableContext.AddLambdaCapture(localVarOutsideLambdaResult.VarName, localVarOutsideLambdaResult.TypeSymbol);
                            return AnalyzeMemberExpLocParent(memberExp, new R.LambdaMemberVar(localVarOutsideLambdaResult.VarName), localVarOutsideLambdaResult.TypeSymbol);

                        case IdentifierResult.LocalVar localVarResult:
                            {
                                R.Loc loc = new R.LocalVarLoc(new R.Name.Normal(localVarResult.VarName));
                                if (localVarResult.IsRef)
                                    loc = new R.DerefLocLoc(loc);

                                return AnalyzeMemberExpLocParent(memberExp, loc, localVarResult.TypeSymbol);
                            }

                        case IdentifierResult.GlobalVar globalVarResult:
                            {
                                R.Loc loc = new R.GlobalVarLoc(globalVarResult.VarName);
                                if (globalVarResult.IsRef)
                                    loc = new R.DerefLocLoc(loc);

                                return AnalyzeMemberExpLocParent(memberExp, loc, globalVarResult.TypeSymbol);
                            }

                        // F.x 는
                        case IdentifierResult.Funcs:
                            globalContext.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp);
                            throw new UnreachableCodeException();                            
                        
                        case IdentifierResult.Type typeResult:
                            return AnalyzeMemberExpTypeParent(memberExp, typeResult.TypeSymbol, memberExp.MemberName, memberExp.MemberTypeArgs);
                        
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
                            return AnalyzeMemberExpTypeParent(memberExp, typeResult.TypeSymbol, memberExp.MemberName, memberExp.MemberTypeArgs);

                        case ExpResult.EnumElem:
                            globalContext.AddFatalError(A2009_ResolveIdentifier_EnumElemCantHaveMember, memberExp);
                            break;

                        case ExpResult.Funcs:
                            globalContext.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp);
                            break;

                        case ExpResult.Exp expResult:
                            return AnalyzeMemberExpLocParent(memberExp, new R.TempLoc(expResult.Result, expResult.TypeSymbol.MakeRPath()), expResult.TypeValue);

                        case ExpResult.Loc locResult:
                            return AnalyzeMemberExpLocParent(memberExp, locResult.Result, locResult.TypeSymbol);
                    }

                    throw new UnreachableCodeException();
                }


                //var memberExpAnalyzer = new MemberExpAnalyzer(analyzer, memberExp);
                //var result = memberExpAnalyzer.Analyze();

                //if (result != null)
                //{
                //    context.AddNodeInfo(memberExp, result.Value.MemberExpInfo);
                //    outTypeValue = result.Value.TypeSymbol;
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
                ITypeSymbol? curElemTypeValue = (listExp.ElemType != null) ? globalContext.GetSymbolByTypeExp(listExp.ElemType) : null;

                foreach (var elem in listExp.Elems)
                {
                    var elemResult = AnalyzeExp_Exp(elem, ResolveHint.None);

                    builder.Add(elemResult.Result);

                    if (curElemTypeValue == null)
                    {
                        curElemTypeValue = elemResult.TypeSymbol;
                        continue;
                    }

                    if (!EqualityComparer<ITypeSymbol>.Default.Equals(curElemTypeValue, elemResult.TypeSymbol))
                    {
                        // TODO: 둘의 공통 조상을 찾아야 하는지 결정을 못했다..
                        globalContext.AddFatalError(A1702_ListExp_MismatchBetweenElementTypes, elem);
                    }
                }

                if (curElemTypeValue == null)
                    globalContext.AddFatalError(A1701_ListExp_CantInferElementTypeWithEmptyElement, listExp);

                var rtype = curElemTypeValue.MakeRPath();

                return new ExpResult.Exp(
                    new R.ListExp(rtype, builder.ToImmutable()),
                    globalContext.GetListType(curElemTypeValue));
            }

            ExpResult.Exp AnalyzeNewExp(S.NewExp newExp)
            {
                var classTypeValue = globalContext.GetSymbolByTypeExp(newExp.Type) as ClassSymbol;
                if (classTypeValue == null)
                    globalContext.AddFatalError(A2601_NewExp_TypeIsNotClass, newExp.Type);

                // NOTICE: 생성자 검색 (AnalyzeCallExpTypeCallable 부분과 비슷)
                var result = classTypeValue.QueryMember(M.Name.Constructor, typeParamCount: 0); // NOTICE: constructor는 타입 파라미터가 없다
                switch (result)
                {
                    case SymbolQueryResult.Constructors constructorResult:
                        var funcMatchResult = FuncMatcher.Match(globalContext, callableContext, localContext, classTypeValue.MakeTypeEnv(), constructorResult.ConstructorInfos, newExp.Args, default);

                        switch(funcMatchResult)
                        {
                            case FuncMatchResult<IModuleConstructorDecl>.MultipleCandidates:
                                globalContext.AddFatalError(A2603_NewExp_MultipleMatchedClassConstructors, newExp);
                                break;

                            case FuncMatchResult<IModuleConstructorDecl>.NotFound:
                                globalContext.AddFatalError(A2602_NewExp_NoMatchedClassConstructor, newExp);
                                break;

                            case FuncMatchResult<IModuleConstructorDecl>.Success matchedConstructor:
                                var constructorValue = globalContext.MakeConstructorValue(constructorResult.Outer, matchedConstructor.CallableInfo);

                                if (!CanAccess(constructorValue))
                                    globalContext.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, newExp);

                                return new ExpResult.Exp(
                                    new R.NewClassExp(
                                        classTypeValue.MakeRPath(),
                                        constructorValue.MakeRPath().ParamHash,
                                        matchedConstructor.Args
                                    ),
                                    classTypeValue);
                        }

                        break;

                    case SymbolQueryResult.NotFound:
                        globalContext.AddFatalError(A2602_NewExp_NoMatchedClassConstructor, newExp);
                        break;

                    case SymbolQueryResult.Error errorResult:
                        HandleItemQueryResultError(globalContext, errorResult, newExp);
                        break;
                }
                
                throw new UnreachableCodeException();
            }

            public ExpResult AnalyzeExp(S.Exp exp, ResolveHint hint)
            {
                switch (exp)
                {
                    case S.IdentifierExp idExp: return AnalyzeIdExp(idExp, hint);
                    case S.NullLiteralExp nullExp: return AnalyzeNullLiteralExp(nullExp, hint);
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
                        return new ExpResult.Loc(new R.TempLoc(expResult.Result, expResult.TypeSymbol.MakeRPath()), expResult.TypeValue);

                    case ExpResult.Loc locResult:
                        return locResult;
                }

                throw new UnreachableCodeException();
            }

            public static R.Exp AnalyzeExp_Exp(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext, S.Exp exp, ResolveHint hint)
            {
                var analyzer = new StmtAndExpAnalyzer(globalContext, callableContext, localContext);
                return analyzer.AnalyzeExp_Exp(exp, hint);
            }

            internal R.Exp AnalyzeExp_Exp(S.Exp exp, ResolveHint hint)
            {
                var result = AnalyzeExp(exp, hint);
                switch (result)
                {
                    case ExpResult.Exp expResult:
                        return expResult.Result;

                    case ExpResult.Loc locResult:
                        return new R.LoadExp(locResult.Result);

                    case ExpResult.Type typeResult:
                        
                        if (typeResult.TypeSymbol is EnumElemSymbol) // E.F
                        {
                            Debug.Assert(false);
                            // 여기서 처리할게 아닌거 같다
                            // EnumElemSymbol이면 ExpResult.Exp 로 들어와야 한다

                            //if (enumElem.IsStandalone())
                            //    return new R.NewEnumElemExp(enumElem, default);
                            //else // 함수로 
                            //    throw new NotImplementedException();
                        }
                        globalContext.AddFatalError(A2008_ResolveIdentifier_CantUseTypeAsExpression, exp);
                        throw new UnreachableCodeException();

                    default:
                        throw new UnreachableCodeException();
                }
            }

            R.StringExpElement AnalyzeStringExpElement(S.StringExpElement elem)
            {
                var stringType = globalContext.GetStringType();

                if (elem is S.ExpStringExpElement expElem)
                {
                    var exp = AnalyzeExp_Exp(expElem.Exp, ResolveHint.None);
                    var expType = exp.GetTypeSymbol();

                    // 캐스팅이 필요하다면 
                    if (globalContext.IsInt(expType))
                    {
                        return new R.ExpStringExpElement(
                            new R.CallInternalUnaryOperatorExp(
                                R.InternalUnaryOperator.ToString_Int_String,
                                exp,
                                stringType
                            )
                        );
                    }
                    else if (globalContext.IsBool(expType))
                    {
                        return new R.ExpStringExpElement(
                                new R.CallInternalUnaryOperatorExp(
                                R.InternalUnaryOperator.ToString_Bool_String,
                                exp,
                                stringType
                            )
                        );
                    }
                    else if (globalContext.IsString(expType))
                    {
                        return new R.ExpStringExpElement(exp);
                    }
                    else
                    {
                        // TODO: ToString
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
            internal R.Exp CastExp_Exp(R.Exp exp, ITypeSymbol expectedType, S.ISyntaxNode nodeForErrorReport) // throws AnalyzeFatalException
            {
                var result = globalContext.TryCastExp_Exp(exp, expectedType);
                if (result != null) return result;

                globalContext.AddFatalError(A2201_Cast_Failed, nodeForErrorReport);
                throw new UnreachableCodeException();
            }
        }
    }
}
