﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using static Gum.IR0Translator.Analyzer;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;
using Pretune;
using System.Linq;
using Gum.Infra;
using Gum.Collections;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        [AutoConstructor]
        partial struct StmtResult
        {
            public R.Stmt Stmt { get; }
        }

        // StmtAndExpAnalyzer
        partial struct StmtAndExpAnalyzer
        {   
            // CommandStmt에 있는 expStringElement를 분석한다
            StmtResult AnalyzeCommandStmt(S.CommandStmt cmdStmt)
            {
                var builder = ImmutableArray.CreateBuilder<R.StringExp>();
                foreach (var cmd in cmdStmt.Commands)
                {
                    var expResult = AnalyzeStringExp(cmd);
                    Debug.Assert(expResult.Exp is R.StringExp);
                    builder.Add((R.StringExp)expResult.Exp);
                }

                return new StmtResult(new R.CommandStmt(builder.ToImmutable()));
            }
            
            StmtResult AnalyzeLocalVarDeclStmt(S.VarDeclStmt varDeclStmt)
            {
                var localVarDecl = AnalyzeLocalVarDecl(varDeclStmt.VarDecl);
                return new StmtResult(new R.LocalVarDeclStmt(localVarDecl));
            }

            //bool AnalyzeIfTestEnumStmt(
            //    IdentifierInfo varIdInfo,
            //    Exp target, 
            //    S.Stmt thenBody,
            //    S.Stmt? elseBody,
            //    TypeValue targetType, EnumElemTypeValue enumElem, [NotNullWhen(true)] out Stmt? outStmt)
            //{
            //    bool bResult = true;
            //    Stmt? ir0ThenBody = null;
            //    Stmt? ir0ElseBody = null;

            //    context.ExecInLocalScope(() =>
            //    {
            //        context.AddOverrideVarInfo(varIdInfo.StorageInfo, targetType);

            //        if (!AnalyzeStmt(thenBody, out ir0ThenBody))
            //            bResult = false;

            //        if (elseBody != null)
            //            if (!AnalyzeStmt(elseBody, out ir0ElseBody))
            //                bResult = false;
            //    });

            //    if (bResult != false)
            //    {
            //        Debug.Assert(ir0ThenBody != null);

            //        var targetTypeId = context.GetTypeId(targetType);
            //        outStmt = new IfTestEnumStmt(new ExpInfo(target, targetTypeId), enumElem.Name, ir0ThenBody, ir0ElseBody);
            //        return true;
            //    }
            //    else
            //    {
            //        outStmt = null;
            //        return false;
            //    }
            //}

            //bool AnalyzeIfTestClassStmt(
            //    IdentifierInfo.Var varIdInfo,
            //    Exp target,
            //    S.Stmt thenBody,
            //    S.Stmt? elseBody,
            //    TypeValue targetType,
            //    TypeValue testType,
            //    Context context, 
            //    [NotNullWhen(true)] out Stmt? outStmt)
            //{
            //    bool bResult = true;
            //    Stmt? ir0ThenBody = null;
            //    Stmt? ir0ElseBody = null;

            //    context.ExecInLocalScope(() =>
            //    {
            //        context.AddOverrideVarInfo(varIdInfo.StorageInfo, testType);

            //        if (!AnalyzeStmt(thenBody, out ir0ThenBody))
            //            bResult = false;

            //        if (elseBody != null)
            //            if (!AnalyzeStmt(elseBody, out ir0ElseBody))
            //                bResult = false;
            //    });

            //    if (bResult)
            //    {
            //        Debug.Assert(ir0ThenBody != null);
            //        var targetTypeId = context.GetTypeId(targetType);
            //        var testTypeId = context.GetTypeId(testType);
            //        outStmt = new IfTestClassStmt(new ExpInfo(target, targetTypeId), testTypeId, ir0ThenBody, ir0ElseBody);
            //        return true;
            //    }
            //    else
            //    {
            //        outStmt = null;
            //        return false;
            //    }
            //}

            StmtResult AnalyzeIfTestStmt(S.IfStmt ifStmt)
            {
                throw new NotImplementedException();

                // TODO: if (Type v = exp as Type) 구문 추가
                // var condResult = AnalyzeExp(ifStmt.Cond, ResolveHint.None);



                //if (idResult is locResult && locResult.Loc is R.LocalVarLoc)
                //{
                //    var testTypeValue = context.GetTypeValueByTypeExp(ifStmt.TestType!);
                //    throw new NotImplementedException();

                //    //if (testTypeValue is EnumElemTypeValue enumElem)
                //    //{
                //    //    return AnalyzeIfTestEnumStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, enumElem, out outStmt);                
                //    //}
                //    //else if (testTypeValue is NormalTypeValue normal)
                //    //{
                //    //    return AnalyzeIfTestClassStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, testTypeValue, out outStmt);
                //    //}
                //    //else
                //    //{
                //    //    context.AddError(A1003_IfStmt_TestTypeShouldBeEnumOrClass, testTypeExp, "if (exp is Test) 구문은 Test부분이 타입이거나 enum값이어야 합니다");
                //    //    return false;
                //    //}
                //}
                //else
                //{
                //    context.AddFatalError(A1001_IfStmt_TestTargetShouldBeLocalVariable, ifStmt.Cond);
                //    throw new UnreachableCodeException();
                //}
            }

            StmtResult AnalyzeIfStmt(S.IfStmt ifStmt)
            {
                if (ifStmt.TestType != null)
                    return AnalyzeIfTestStmt(ifStmt);

                // 순회
                var condResult = AnalyzeExp_Exp(ifStmt.Cond, ResolveHint.None);
                var bodyResult = AnalyzeStmt(ifStmt.Body);
                StmtResult? elseBodyResult = (ifStmt.ElseBody != null) ? AnalyzeStmt(ifStmt.ElseBody) : null;

                // Analyzer 처리
                if (!globalContext.IsAssignable(globalContext.GetBoolType(), condResult.TypeValue))
                    globalContext.AddFatalError(A1004_IfStmt_ConditionShouldBeBool, ifStmt.Cond);

                return new StmtResult(new R.IfStmt(condResult.Exp, bodyResult.Stmt, elseBodyResult?.Stmt));
            }

            [AutoConstructor]
            partial struct ForStmtInitializerResult
            {
                public R.ForStmtInitializer Initializer { get; }
            }

            ForStmtInitializerResult AnalyzeForStmtInitializer(S.ForStmtInitializer forInit)
            {
                switch (forInit)
                {
                    case S.VarDeclForStmtInitializer varDeclInit:
                        var localVarDecl = AnalyzeLocalVarDecl(varDeclInit.VarDecl);
                        return new ForStmtInitializerResult(new R.VarDeclForStmtInitializer(localVarDecl));

                    case S.ExpForStmtInitializer expInit:
                        var expResult = AnalyzeTopLevelExp_Exp(expInit.Exp, ResolveHint.None, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall);
                        return new ForStmtInitializerResult(new R.ExpForStmtInitializer(expResult.Exp));

                    default:
                        throw new NotImplementedException();
                }
            }

            StmtResult AnalyzeForStmt(S.ForStmt forStmt)
            {
                var newAnalyzer = NewAnalyzer();
                
                R.ForStmtInitializer? initializer = null;
                if (forStmt.Initializer != null)
                {
                    var initializerResult = newAnalyzer.AnalyzeForStmtInitializer(forStmt.Initializer);
                    initializer = initializerResult.Initializer;
                }

                R.Exp? cond = null;
                if (forStmt.CondExp != null)
                {
                    // 밑에서 쓰이므로 분석실패시 종료
                    var condResult = newAnalyzer.AnalyzeExp_Exp(forStmt.CondExp, ResolveHint.None);

                    // 에러가 나면 에러를 추가하고 계속 진행
                    if (!globalContext.IsAssignable(globalContext.GetBoolType(), condResult.TypeValue))
                        globalContext.AddError(A1101_ForStmt_ConditionShouldBeBool, forStmt.CondExp);

                    cond = condResult.Exp;
                }

                R.Exp? continueInfo = null;
                if (forStmt.ContinueExp != null)
                {
                    var continueResult = newAnalyzer.AnalyzeTopLevelExp_Exp(forStmt.ContinueExp, ResolveHint.None, A1103_ForStmt_ContinueExpShouldBeAssignOrCall);
                    var contExpType = continueResult.TypeValue.GetRType();
                    continueInfo = continueResult.Exp;
                }
                
                var newLoopStmtAnalyzer = newAnalyzer.NewAnalyzerWithLoop();
                var bodyResult = newLoopStmtAnalyzer.AnalyzeStmt(forStmt.Body);

                return new StmtResult(new R.ForStmt(initializer, cond, continueInfo, bodyResult.Stmt));
            }

            StmtResult AnalyzeContinueStmt(S.ContinueStmt continueStmt)
            {
                if (!localContext.IsInLoop())
                    globalContext.AddFatalError(A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);

                return new StmtResult(R.ContinueStmt.Instance);
            }

            StmtResult AnalyzeBreakStmt(S.BreakStmt breakStmt)
            {
                if (!localContext.IsInLoop())
                {
                    globalContext.AddFatalError(A1601_BreakStmt_ShouldUsedInLoop, breakStmt);
                }

                return new StmtResult(R.BreakStmt.Instance);
            }

            StmtResult AnalyzeReturnStmt(S.ReturnStmt returnStmt)
            {
                // seq 함수는 여기서 모두 처리 
                if (callableContext.IsSeqFunc())
                {
                    if (returnStmt.Value != null)
                        globalContext.AddFatalError(A1202_ReturnStmt_SeqFuncShouldReturnVoid, returnStmt);

                    return new StmtResult(new R.ReturnStmt(null));
                }

                // 리턴 값이 없을 경우
                if (returnStmt.Value == null)
                {
                    var retTypeValue = callableContext.GetRetTypeValue();
                    var voidTypeValue = globalContext.GetVoidType();

                    if (retTypeValue == null)
                    {
                        callableContext.SetRetTypeValue(voidTypeValue);
                    }
                    else if (retTypeValue != voidTypeValue)
                    {
                        globalContext.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                    }

                    return new StmtResult(new R.ReturnStmt(null));
                }
                else
                {
                    // 이 함수의 적혀져 있던 리턴타입 or 첫번째로 발견되서 유지되고 있는 리턴타입
                    var retTypeValue = callableContext.GetRetTypeValue();

                    if (retTypeValue == null)
                    {
                        // 힌트타입 없이 분석
                        var valueResult = AnalyzeExp_Exp(returnStmt.Value, ResolveHint.None);

                        // 리턴값이 안 적혀 있었으므로 적는다
                        callableContext.SetRetTypeValue(valueResult.TypeValue);

                        return new StmtResult(new R.ReturnStmt(valueResult.Exp));
                    }
                    else
                    {
                        // 리턴타입을 힌트로 사용한다
                        var valueResult = AnalyzeExp_Exp(returnStmt.Value, ResolveHint.Make(retTypeValue));

                        // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                        if (!globalContext.IsAssignable(retTypeValue, valueResult.TypeValue))
                            globalContext.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt.Value);

                        return new StmtResult(new R.ReturnStmt(valueResult.Exp));
                    }
                }
            }

            StmtResult AnalyzeBlockStmt(S.BlockStmt blockStmt)
            {
                bool bFatal = false;
                var builder = ImmutableArray.CreateBuilder<R.Stmt>();

                var newStmtAnalyzer = NewAnalyzer();

                foreach (var stmt in blockStmt.Stmts)
                {
                    try
                    {
                        var stmtResult = newStmtAnalyzer.AnalyzeStmt(stmt);
                        builder.Add(stmtResult.Stmt);
                    }
                    catch (FatalAnalyzeException)
                    {
                        bFatal = true;
                    }
                }

                if (bFatal)
                    throw new FatalAnalyzeException();

                return new StmtResult(new R.BlockStmt(builder.ToImmutable()));
            }

            StmtResult AnalyzeBlankStmt()
            {
                return new StmtResult(R.BlankStmt.Instance);
            }

            StmtResult AnalyzeExpStmt(S.ExpStmt expStmt)
            {
                var expResult = AnalyzeTopLevelExp_Exp(expStmt.Exp, ResolveHint.None, A1301_ExpStmt_ExpressionShouldBeAssignOrCall);
                return new StmtResult(new R.ExpStmt(expResult.Exp));
            }

            R.CapturedStatement AnalyzeCapturedStatement(TypeValue? retTypeValue, S.Stmt body)
            {
                // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
                // 파라미터는 람다 함수의 지역변수로 취급한다                
                var newLambdaContext = new LambdaContext(localContext, retTypeValue);
                var newLocalContext = new LocalContext(newLambdaContext);
                var newAnalyzer = new StmtAndExpAnalyzer(globalContext, newLambdaContext, newLocalContext);

                // 본문 분석
                var bodyResult = newAnalyzer.AnalyzeStmt(body);                
                var capturedLocalVars = newLambdaContext.GetCapturedLocalVars();

                // TODO: need capture this확인해서 this 넣기
                // var bCaptureThis = newLambdaContext.NeedCaptureThis();
                R.Path? capturedThisType = null;                

                return new R.CapturedStatement(capturedThisType, capturedLocalVars, bodyResult.Stmt);
            }

            StmtResult AnalyzeTaskStmt(S.TaskStmt taskStmt)
            {
                var capturedStatement = AnalyzeCapturedStatement(globalContext.GetVoidType(), taskStmt.Body);
                return new StmtResult(new R.TaskStmt(capturedStatement));
            }

            StmtResult AnalyzeAwaitStmt(S.AwaitStmt awaitStmt)
            {                
                var newStmtAnalyzer = NewAnalyzer();

                var bodyResult = newStmtAnalyzer.AnalyzeStmt(awaitStmt.Body);
                return new StmtResult(new R.AwaitStmt(bodyResult.Stmt));
            }

            StmtResult AnalyzeAsyncStmt(S.AsyncStmt asyncStmt)
            {
                var capturedStatement = AnalyzeCapturedStatement(globalContext.GetVoidType(), asyncStmt.Body);
                return new StmtResult(new R.AsyncStmt(capturedStatement));
            }

            StmtResult AnalyzeForeachStmt(S.ForeachStmt foreachStmt)
            {
                throw new NotImplementedException();

                //// iterator
                //var iteratorResult = AnalyzeExp(foreachStmt.Iterator, null);
                //var elemType = context.GetTypeValueByTypeExp(foreachStmt.Type);

                //// 1. seq<T> constraint
                //// 2. seq<T> 인터페이스를 가지고 있는 struct

                //// list<T> => IEnumerable<T>
                //// interface IEnumerable<T>
                //// {            
                ////     seq<int> GetEnumerator(); // 공변 반환형
                //// }


                //// SeqTypeValue(NormalTypeValue) // seq<T>인터페이스를 가지고 있는 struct
                //// BoxTypeValue(SeqTypeValue(NormalTypeValue)) // 
                //// RefTypeValue(SeqTypeValue(NormalTypeValue)) // 
                //// InterfaceTypeValue(SeqTypeValue(NormalTypeValue)) // 네가지



                //TypeValue? iteratorElemType = null;
                //if (iteratorResult.TypeValue is NormalTypeValue normalIteratorType)
                //{
                //    var typeId = normalIteratorType.GetTypeId( );

                //    if (ModuleInfoEqualityComparer.EqualsItemId(typeId, ItemIds.List))
                //    {
                //        iteratorElemType = normalIteratorType.Entry.TypeArgs[0];
                //    }
                //    else if (ModuleInfoEqualityComparer.EqualsItemId(typeId, ItemIds.Enumerable))
                //    {
                //        iteratorElemType = normalIteratorType.Entry.TypeArgs[0];
                //    }
                //    else
                //    {
                //        context.AddFatalError(A1801_ForeachStmt_IteratorShouldBeListOrEnumerable, foreachStmt.Iterator);
                //    }
                //}
                //else
                //{
                //    context.AddFatalError(A1801_ForeachStmt_IteratorShouldBeListOrEnumerable, foreachStmt.Iterator);
                //}

                //if (elemType is VarTypeValue) // var 라면, iteratorElemType을 쓴다
                //{
                //    elemType = iteratorElemType;
                //}
                //else // 아니라면 둘이 호환되는지 확인한다
                //{
                //    // TODO: Cast
                //    if (!context.IsAssignable(elemType, iteratorElemType))
                //        context.AddFatalError(A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType, foreachStmt);
                //}

                //var stmt = context.ExecInLocalScope(() =>
                //{
                //    context.AddLocalVarInfo(foreachStmt.VarName, elemType);

                //    return context.ExecInLoop(() =>
                //    {
                //        var bodyResult = AnalyzeStmt(foreachStmt.Body);

                //        var elemRType = elemType.GetRType();
                //        var iteratorRType = iteratorResult.TypeValue.GetRType();

                //        return new R.ForeachStmt(elemRType, foreachStmt.VarName, new R.ExpInfo(iteratorResult.Exp, iteratorRType), bodyResult.Stmt);
                //    });
                //});

                //return new StmtResult(stmt);
            }

            StmtResult AnalyzeYieldStmt(S.YieldStmt yieldStmt)
            {
                if (!callableContext.IsSeqFunc())
                    globalContext.AddFatalError(A1401_YieldStmt_YieldShouldBeInSeqFunc, yieldStmt);

                // yield에서는 retType이 명시되는 경우만 있을 것이다
                var retTypeValue = callableContext.GetRetTypeValue();
                Debug.Assert(retTypeValue != null);

                // NOTICE: 리턴 타입을 힌트로 넣었다
                var valueResult = AnalyzeExp_Exp(yieldStmt.Value, ResolveHint.Make(retTypeValue));

                if (!globalContext.IsAssignable(retTypeValue, valueResult.TypeValue))
                    globalContext.AddFatalError(A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType, yieldStmt.Value);

                return new StmtResult(new R.YieldStmt(valueResult.Exp));
            }            

            public StmtResult AnalyzeStmt(S.Stmt stmt)
            {   
                switch (stmt)
                {
                    case S.VarDeclStmt varDeclStmt: return AnalyzeLocalVarDeclStmt(varDeclStmt);
                    case S.CommandStmt cmdStmt: return AnalyzeCommandStmt(cmdStmt);
                    case S.IfStmt ifStmt: return AnalyzeIfStmt(ifStmt);
                    case S.ForStmt forStmt: return AnalyzeForStmt(forStmt);
                    case S.ContinueStmt continueStmt: return AnalyzeContinueStmt(continueStmt);
                    case S.BreakStmt breakStmt: return AnalyzeBreakStmt(breakStmt);
                    case S.ReturnStmt returnStmt: return AnalyzeReturnStmt(returnStmt);
                    case S.BlockStmt blockStmt: return AnalyzeBlockStmt(blockStmt);
                    case S.BlankStmt blankStmt: return AnalyzeBlankStmt();
                    case S.ExpStmt expStmt: return AnalyzeExpStmt(expStmt);
                    case S.TaskStmt taskStmt: return AnalyzeTaskStmt(taskStmt);
                    case S.AwaitStmt awaitStmt: return AnalyzeAwaitStmt(awaitStmt);
                    case S.AsyncStmt asyncStmt: return AnalyzeAsyncStmt(asyncStmt);
                    case S.ForeachStmt foreachStmt: return AnalyzeForeachStmt(foreachStmt);
                    case S.YieldStmt yieldStmt: return AnalyzeYieldStmt(yieldStmt);
                    default: throw new UnreachableCodeException();
                }
            }

            bool IsTopLevelExp(S.Exp exp)
            {
                switch (exp)
                {
                    case S.UnaryOpExp unOpExp:
                        return unOpExp.Kind == S.UnaryOpKind.PostfixInc ||
                            unOpExp.Kind == S.UnaryOpKind.PostfixDec ||
                            unOpExp.Kind == S.UnaryOpKind.PrefixInc ||
                            unOpExp.Kind == S.UnaryOpKind.PrefixDec;

                    case S.BinaryOpExp binOpExp:
                        return binOpExp.Kind == S.BinaryOpKind.Assign;

                    case S.CallExp _:
                        return true;

                    default:
                        return false;
                }
            }

            ExpExpResult AnalyzeTopLevelExp_Exp(S.Exp exp, ResolveHint hint, AnalyzeErrorCode code)
            {
                if (!IsTopLevelExp(exp))
                    globalContext.AddFatalError(code, exp);

                return AnalyzeExp_Exp(exp, hint);
            }

        }
    }
}