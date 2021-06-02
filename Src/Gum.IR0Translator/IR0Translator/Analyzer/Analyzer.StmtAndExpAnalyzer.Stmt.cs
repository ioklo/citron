using System;
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
                    Debug.Assert(expResult.Result is R.StringExp);
                    builder.Add((R.StringExp)expResult.Result);
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

            // 
            StmtResult AnalyzeIfTestStmt(S.IfTestStmt ifTestStmt)
            {
                // if (e is Type varName) body 
                // IfTestStmt -> IfTestClassStmt, IfTestEnumElemStmt

                var targetResult = AnalyzeExp_Loc(ifTestStmt.Exp, ResolveHint.None);
                var testType = globalContext.GetTypeValueByTypeExp(ifTestStmt.TestType);                

                if (testType is EnumElemTypeValue enumElemType)
                {
                    // exact match
                    if (!enumElemType.Outer.Equals(targetResult.TypeValue))
                        globalContext.AddFatalError(A2301_IfTestStmt_CantDowncast, ifTestStmt.Exp);

                    // analyze body
                    StmtResult bodyResult;
                    if (ifTestStmt.VarName != null)
                    {
                        var newAnalyzer = NewAnalyzer();
                        newAnalyzer.localContext.AddLocalVarInfo(ifTestStmt.VarName, enumElemType);

                        bodyResult = newAnalyzer.AnalyzeStmt(ifTestStmt.Body);
                    }
                    else
                    {
                        bodyResult = AnalyzeStmt(ifTestStmt.Body);
                    }

                    // analyze elseBody
                    StmtResult? elseBodyResult;
                    if (ifTestStmt.ElseBody != null)
                    {
                        elseBodyResult = AnalyzeStmt(ifTestStmt.ElseBody);
                    }
                    else
                    {
                        elseBodyResult = null;
                    }

                    var rstmt = new R.IfTestEnumElemStmt(targetResult.Result, enumElemType.GetRPath_Nested(), ifTestStmt.VarName, bodyResult.Stmt, elseBodyResult?.Stmt);
                    return new StmtResult(rstmt);
                }
                else
                {
                    throw new NotImplementedException();
                }






                // TODO: if (exp is Type v) 구문 추가
                // var condResult = AnalyzeExp(ifStmt.Cond, ResolveHint.None);


                //if (idResult is locResult && locResult.Result is R.LocalVarLoc)
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
                // 순회
                var condResult = AnalyzeExp_Exp(ifStmt.Cond, ResolveHint.None);
                var bodyResult = AnalyzeStmt(ifStmt.Body);
                StmtResult? elseBodyResult = (ifStmt.ElseBody != null) ? AnalyzeStmt(ifStmt.ElseBody) : null;
                
                condResult = CastExp_Exp(condResult, globalContext.GetBoolType(), ifStmt.Cond);

                return new StmtResult(new R.IfStmt(condResult.Result, bodyResult.Stmt, elseBodyResult?.Stmt));
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
                        return new ForStmtInitializerResult(new R.ExpForStmtInitializer(expResult.Result));

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
                    
                    try
                    {
                        condResult = CastExp_Exp(condResult, globalContext.GetBoolType(), forStmt.CondExp);
                        cond = condResult.Result;
                    }
                    catch(AnalyzerFatalException)
                    {
                        // 에러가 나도 계속 진행
                    }
                }

                R.Exp? continueInfo = null;
                if (forStmt.ContinueExp != null)
                {
                    var continueResult = newAnalyzer.AnalyzeTopLevelExp_Exp(forStmt.ContinueExp, ResolveHint.None, A1103_ForStmt_ContinueExpShouldBeAssignOrCall);
                    var contExpType = continueResult.TypeValue.GetRPath();
                    continueInfo = continueResult.Result;
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

                        return new StmtResult(new R.ReturnStmt(valueResult.Result));
                    }
                    else
                    {
                        // 리턴타입을 힌트로 사용한다
                        var valueResult = AnalyzeExp_Exp(returnStmt.Value, ResolveHint.Make(retTypeValue));

                        // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                        valueResult = CastExp_Exp(valueResult, retTypeValue, returnStmt.Value);

                        return new StmtResult(new R.ReturnStmt(valueResult.Result));
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
                    catch (AnalyzerFatalException)
                    {
                        bFatal = true;
                    }
                }

                if (bFatal)
                    throw new AnalyzerFatalException();

                return new StmtResult(new R.BlockStmt(builder.ToImmutable()));
            }

            StmtResult AnalyzeBlankStmt()
            {
                return new StmtResult(R.BlankStmt.Instance);
            }

            StmtResult AnalyzeExpStmt(S.ExpStmt expStmt)
            {
                var expResult = AnalyzeTopLevelExp_Exp(expStmt.Exp, ResolveHint.None, A1301_ExpStmt_ExpressionShouldBeAssignOrCall);
                return new StmtResult(new R.ExpStmt(expResult.Result));
            }

            R.Name AnalyzeCapturedStatement(TypeValue? retTypeValue, S.Stmt body)
            {
                var newAnonymousId = callableContext.NewAnonymousId();

                // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
                // 파라미터는 람다 함수의 지역변수로 취급한다                
                var newLambdaContext = new LambdaContext(callableContext, localContext, newAnonymousId, retTypeValue);
                var newLocalContext = new LocalContext();
                var newAnalyzer = new StmtAndExpAnalyzer(globalContext, newLambdaContext, newLocalContext);

                // 본문 분석
                var bodyResult = newAnalyzer.AnalyzeStmt(body);                
                var capturedLocalVars = newLambdaContext.GetCapturedLocalVars();

                // TODO: need capture this확인해서 this 넣기
                // var bCaptureThis = newLambdaContext.NeedCaptureThis();
                R.Path? capturedThisType = null;                

                var decl = new R.CapturedStatementDecl(new R.Name.Anonymous(newAnonymousId), new R.CapturedStatement(capturedThisType, capturedLocalVars, bodyResult.Stmt));
                callableContext.AddDecl(decl);

                return new R.Name.Anonymous(newAnonymousId);
            }

            StmtResult AnalyzeTaskStmt(S.TaskStmt taskStmt)
            {
                var capturedStatementName = AnalyzeCapturedStatement(globalContext.GetVoidType(), taskStmt.Body);
                var path = callableContext.GetPath(capturedStatementName, R.ParamHash.None, default);
                return new StmtResult(new R.TaskStmt(path));
            }

            StmtResult AnalyzeAwaitStmt(S.AwaitStmt awaitStmt)
            {                
                var newStmtAnalyzer = NewAnalyzer();

                var bodyResult = newStmtAnalyzer.AnalyzeStmt(awaitStmt.Body);
                return new StmtResult(new R.AwaitStmt(bodyResult.Stmt));
            }

            StmtResult AnalyzeAsyncStmt(S.AsyncStmt asyncStmt)
            {
                var capturedStatementName = AnalyzeCapturedStatement(globalContext.GetVoidType(), asyncStmt.Body);
                var path = callableContext.GetPath(capturedStatementName, R.ParamHash.None, default);
                return new StmtResult(new R.AsyncStmt(path));
            }

            StmtResult AnalyzeForeachStmt(S.ForeachStmt foreachStmt)
            {
                // SYNTAX: foreach(var elemVarName in iterator) body
                // IR0: foreach(type elemVarName in iteratorLoc)
                var iteratorResult = AnalyzeExp_Loc(foreachStmt.Iterator, ResolveHint.None);
                
                // 먼저, iteratorResult가 anonymous_seq타입인지 확인한다
                if (iteratorResult.TypeValue is SeqTypeValue seqIteratorType)
                {
                    var elemType = globalContext.GetTypeValueByTypeExp(foreachStmt.Type);
                    
                    if (elemType is VarTypeValue) // var type처리
                    {
                        elemType = seqIteratorType.YieldType;
                    }
                    else
                    {
                        // 완전히 같은지 체크
                        if (elemType.Equals(seqIteratorType.YieldType))
                        {
                            // relemType = elemType.GetRPath();
                        }
                        else
                        {
                            // 다르다면 seq_cast 시도
                            // foreach(type elemName in seq_cast(type, iteratorLoc))
                            throw new NotImplementedException();
                        }
                    }

                    // 루프 컨텍스트를 하나 열고
                    var loopAnalyzer = NewAnalyzerWithLoop();

                    // 루프 컨텍스트에 로컬을 하나 추가하고
                    loopAnalyzer.localContext.AddLocalVarInfo(foreachStmt.VarName, elemType);

                    // 본문 분석
                    var bodyResult = loopAnalyzer.AnalyzeStmt(foreachStmt.Body);

                    var rforeachStmt = new R.ForeachStmt(elemType.GetRPath(), foreachStmt.VarName, iteratorResult.Result, bodyResult.Stmt);
                    return new StmtResult(rforeachStmt);
                }
                else
                {
                    // 축약형 처리
                    // anonymous_seq를 리턴하는 식으로 변경해준다
                    // foreach(var i in [1, 2, 3, 4]) => foreach(var i in [1, 2, 3, 4].GetEnumerator())
                    throw new NotImplementedException();
                }
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
                valueResult = CastExp_Exp(valueResult, retTypeValue, yieldStmt.Value);

                return new StmtResult(new R.YieldStmt(valueResult.Result));
            }            

            public StmtResult AnalyzeStmt(S.Stmt stmt)
            {   
                switch (stmt)
                {
                    case S.VarDeclStmt varDeclStmt: return AnalyzeLocalVarDeclStmt(varDeclStmt);
                    case S.CommandStmt cmdStmt: return AnalyzeCommandStmt(cmdStmt);
                    case S.IfStmt ifStmt: return AnalyzeIfStmt(ifStmt);
                    case S.IfTestStmt ifTestStmt: return AnalyzeIfTestStmt(ifTestStmt);
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

            ExpResult.Exp AnalyzeTopLevelExp_Exp(S.Exp exp, ResolveHint hint, AnalyzeErrorCode code)
            {
                if (!IsTopLevelExp(exp))
                    globalContext.AddFatalError(code, exp);

                return AnalyzeExp_Exp(exp, hint);
            }

        }
    }
}
