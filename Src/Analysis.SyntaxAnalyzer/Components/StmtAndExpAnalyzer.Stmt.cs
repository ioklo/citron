using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using static Citron.Analysis.SyntaxAnalysisErrorCode;

using S = Citron.Syntax;
using R = Citron.IR0;
using Pretune;
using System.Linq;
using Citron.Infra;
using Citron.Collections;
using Citron.Module;

namespace Citron.Analysis
{
    [AutoConstructor]
    partial struct StmtResult
    {
        public R.Stmt Stmt { get; }
        public bool bFlowEndCompletely { get; } // 모든 구문에 리턴이 있어서 더이상 진행할 수 없다
    }

    // StmtAndExpAnalyzer
    partial class StmtAndExpAnalyzer
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

            return new StmtResult(new R.CommandStmt(builder.ToImmutable()), bFlowEndCompletely: false);
        }
            
        StmtResult AnalyzeLocalVarDeclStmt(S.VarDeclStmt varDeclStmt)
        {
            var localVarDecl = AnalyzeLocalVarDecl(varDeclStmt.VarDecl);
            return new StmtResult(new R.LocalVarDeclStmt(localVarDecl), bFlowEndCompletely: false);
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
            var testType = globalContext.GetSymbolByTypeExp(ifTestStmt.TestType);                

            if (testType is EnumElemSymbol enumElem)
            {
                // exact match
                if (!targetResult.TypeSymbol.Equals(enumElem.GetOuter()))
                    globalContext.AddFatalError(A2301_IfTestStmt_CantDowncast, ifTestStmt.Exp);

                // analyze body
                StmtResult bodyResult;
                if (ifTestStmt.VarName != null)
                {
                    var newAnalyzer = NewAnalyzer();
                    newAnalyzer.localContext.AddLocalVarInfo(true, enumElem, new Name.Normal(ifTestStmt.VarName));

                    bodyResult = newAnalyzer.AnalyzeStmt(ifTestStmt.Body);
                }
                else
                {
                    bodyResult = AnalyzeStmt(ifTestStmt.Body);
                }

                // analyze elseBody
                StmtResult? elseBodyResult;
                bool bFlowEndCompletely;

                if (ifTestStmt.ElseBody != null)
                {
                    elseBodyResult = AnalyzeStmt(ifTestStmt.ElseBody);
                    bFlowEndCompletely = bodyResult.bFlowEndCompletely && elseBodyResult.Value.bFlowEndCompletely;
                }
                else
                {
                    elseBodyResult = null;
                    bFlowEndCompletely = false;
                }

                var rstmt = new R.IfTestEnumElemStmt(targetResult.Result, enumElem, ifTestStmt.VarName, bodyResult.Stmt, elseBodyResult?.Stmt);
                return new StmtResult(rstmt, bFlowEndCompletely);
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
        
        public void AnalyzeBody(ImmutableArray<S.Stmt> body, S.ISyntaxNode nodeForErrorReport)
        {
            bool bFlowEndCompletely = false;

            // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
            foreach (var stmt in body)
            {
                if (bFlowEndCompletely) // 플로우가 끝났는데도 왔으면 워닝, 계속 진행은 해야 한다
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var result = AnalyzeStmt(stmt);
                    bFlowEndCompletely = result.bFlowEndCompletely;
                }
            }

            // return/throw구문이 없는 상태로 끝났을 경우 리턴 체크
            if (!bFlowEndCompletely)
            {
                // constructor이거나, void리턴인 경우,
                if (bodyContext.IsSetReturn()) // 함수, 람다 등에서 리턴 타입이 확정된 경우
                {
                    var funcReturn = bodyContext.GetReturn();

                    if(funcReturn == null) // constructor 인 경우 통과
                    {
                    }
                    else if(funcReturn.Value.IsRef == false && globalContext.IsVoidType(funcReturn.Value.Type)) // void 리턴인 경우 통과
                    {
                    }
                    else
                    {
                        globalContext.AddFatalError(A2901_BodyShouldReturn, nodeForErrorReport);
                    }
                }
                else // 리턴타입이 확정되지 않은 경우
                {
                    bodyContext.SetReturn(false, globalContext.GetVoidType());
                }
            }
        }

        StmtResult AnalyzeIfStmt(S.IfStmt ifStmt)
        {
            // 순회
            var condExp = AnalyzeExp_Exp(ifStmt.Cond, ResolveHint.None);
            var bodyResult = AnalyzeStmt(ifStmt.Body);

            StmtResult? elseBodyResult;
            bool bFlowEndCompletely;

            if (ifStmt.ElseBody != null)
            {
                elseBodyResult = AnalyzeStmt(ifStmt.ElseBody);
                bFlowEndCompletely = bodyResult.bFlowEndCompletely && elseBodyResult.Value.bFlowEndCompletely;
            }
            else
            {
                elseBodyResult = null;
                bFlowEndCompletely = false;
            }

            condExp = globalContext.TryCastExp_Exp(condExp, globalContext.GetBoolType());

            if (condExp == null)
                globalContext.AddFatalError(A1001_IfStmt_ConditionShouldBeBool, ifStmt.Cond);

            return new StmtResult(new R.IfStmt(condExp, bodyResult.Stmt, elseBodyResult?.Stmt), bFlowEndCompletely);
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
                    // VarDecl의 속성을 여기서 들춰서 분기하면 안될거 같다. AnalyzeLocalVarDecl에서 처리하도록
                    var localVarDecl = AnalyzeLocalVarDecl(varDeclInit.VarDecl);
                    return new ForStmtInitializerResult(new R.VarDeclForStmtInitializer(localVarDecl));

                case S.ExpForStmtInitializer expInit:
                    var exp = AnalyzeTopLevelExp_Exp(expInit.Exp, ResolveHint.None, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall);
                    return new ForStmtInitializerResult(new R.ExpForStmtInitializer(exp));

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
                var rawCond = newAnalyzer.AnalyzeExp_Exp(forStmt.CondExp, ResolveHint.None);
                cond = globalContext.TryCastExp_Exp(rawCond, globalContext.GetBoolType());

                if (cond == null)
                    globalContext.AddFatalError(A1101_ForStmt_ConditionShouldBeBool, forStmt.CondExp);
            }

            R.Exp? continueExp = null;
            if (forStmt.ContinueExp != null)
                continueExp = newAnalyzer.AnalyzeTopLevelExp_Exp(forStmt.ContinueExp, ResolveHint.None, A1103_ForStmt_ContinueExpShouldBeAssignOrCall);
                
            var newLoopStmtAnalyzer = newAnalyzer.NewAnalyzerWithLoop();
            var bodyResult = newLoopStmtAnalyzer.AnalyzeStmt(forStmt.Body);

            return new StmtResult(new R.ForStmt(initializer, cond, continueExp, bodyResult.Stmt), bodyResult.bFlowEndCompletely);
        }

        StmtResult AnalyzeContinueStmt(S.ContinueStmt continueStmt)
        {
            if (!localContext.IsInLoop())
                globalContext.AddFatalError(A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);

            return new StmtResult(new R.ContinueStmt(), bFlowEndCompletely: false);
        }

        StmtResult AnalyzeBreakStmt(S.BreakStmt breakStmt)
        {
            if (!localContext.IsInLoop())
            {
                globalContext.AddFatalError(A1601_BreakStmt_ShouldUsedInLoop, breakStmt);
            }

            return new StmtResult(new R.BreakStmt(), bFlowEndCompletely: false);
        }

        StmtResult AnalyzeReturnStmt(S.ReturnStmt returnStmt)
        {
            // seq 함수는 여기서 모두 처리 
            if (bodyContext.IsSeqFunc())
            {
                if (returnStmt.Info != null)
                    globalContext.AddFatalError(A1202_ReturnStmt_SeqFuncShouldReturnVoid, returnStmt);

                return new StmtResult(new R.ReturnStmt(R.ReturnInfo.None.Instance), bFlowEndCompletely: true);
            }

            // 리턴 값이 없을 경우
            if (returnStmt.Info == null)
            {
                // 리턴값이 이미 정해져 있는 경우
                if (bodyContext.IsSetReturn())
                {
                    var funcReturn = bodyContext.GetReturn();

                    // 생성자거나, void 함수가 아니라면 에러
                    if (funcReturn != null && !globalContext.IsVoidType(funcReturn.Value.Type))
                    {
                        globalContext.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                    }
                }
                else
                {
                    // 처음으로 보이는 return; 으로 이 함수는 void 리턴을 확정 한다.
                    bodyContext.SetReturn(false, globalContext.GetVoidType());
                }
                
                return new StmtResult(new R.ReturnStmt(R.ReturnInfo.None.Instance), bFlowEndCompletely: true);
            }
            else if (returnStmt.Info.Value.IsRef) // return ref i; 또는  () => ref i;
            {                
                if (!bodyContext.IsSetReturn())
                {
                    // 아직 리턴값이 안정해졌다면, 힌트타입 없이 분석
                    var valueResult = AnalyzeExp(returnStmt.Info.Value.Value, ResolveHint.None);

                    switch(valueResult)
                    {
                        // TODO: box
                        case ExpResult.Loc locResult:
                            {
                                // 리턴값이 안 적혀 있었으므로 적는다
                                bodyContext.SetReturn(true, locResult.TypeSymbol);
                                return new StmtResult(new R.ReturnStmt(new R.ReturnInfo.Ref(locResult.Result)), bFlowEndCompletely: true);
                            }

                        default:
                            globalContext.AddFatalError(A1203_ReturnStmt_RefTargetIsNotLocation, returnStmt.Info.Value.Value);
                            throw new UnreachableCodeException();
                    }
                }
                else
                {
                    var funcReturn = bodyContext.GetReturn();

                    if (funcReturn == null || !funcReturn.Value.IsRef)
                        globalContext.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);

                    // ref return은 힌트를 쓰지 않는다
                    var valueResult = AnalyzeExp(returnStmt.Info.Value.Value, ResolveHint.None);

                    switch(valueResult)
                    {
                        // TODO: box
                        case ExpResult.Loc locResult:
                            {
                                // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                                // ExactMatch여야 한다
                                if (!locResult.TypeSymbol.Equals(funcReturn.Value))
                                    globalContext.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);

                                return new StmtResult(new R.ReturnStmt(new R.ReturnInfo.Ref(locResult.Result)), bFlowEndCompletely: true);
                            }

                        default:
                            globalContext.AddFatalError(A1203_ReturnStmt_RefTargetIsNotLocation, returnStmt.Info.Value.Value);
                            throw new UnreachableCodeException();
                    }
                }
            }
            else // 일반적인 리턴 값일 경우
            {
                // 아직 리턴값이 정해지지 않은 경우
                if (!bodyContext.IsSetReturn())
                {
                    // 힌트타입 없이 분석
                    var retValueExp = AnalyzeExp_Exp(returnStmt.Info.Value.Value, ResolveHint.None);

                    // 리턴값이 안 적혀 있었으므로 적는다
                    bodyContext.SetReturn(false, retValueExp.GetTypeSymbol());

                    return new StmtResult(new R.ReturnStmt(new R.ReturnInfo.Expression(retValueExp)), bFlowEndCompletely: true);
                }
                else
                {
                    var funcReturn = bodyContext.GetReturn();

                    // 생성자였다면
                    if (funcReturn == null)
                        throw new NotImplementedException(); // 에러 처리

                    // TODO: ref 고려
                    if (funcReturn.Value.IsRef)
                        throw new NotImplementedException();

                    // 리턴타입을 힌트로 사용한다
                    var retValueExp = AnalyzeExp_Exp(returnStmt.Info.Value.Value, ResolveHint.Make(funcReturn.Value.Type));

                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    retValueExp = CastExp_Exp(retValueExp, funcReturn.Value.Type, returnStmt.Info.Value.Value);

                    return new StmtResult(new R.ReturnStmt(new R.ReturnInfo.Expression(retValueExp)), bFlowEndCompletely: true);
                }
            }
        }

        StmtResult AnalyzeBlockStmt(S.BlockStmt blockStmt)
        {
            bool bFatal = false;
            bool bFlowEndCompletely = false;
            var builder = ImmutableArray.CreateBuilder<R.Stmt>();

            var newStmtAnalyzer = NewAnalyzer();

            foreach (var stmt in blockStmt.Stmts)
            {
                try
                {
                    if (bFlowEndCompletely) // return/throw 이후에 구문이 나오면 워닝 처리하고 계속 진행한다
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var stmtResult = newStmtAnalyzer.AnalyzeStmt(stmt);
                        builder.Add(stmtResult.Stmt);
                        bFlowEndCompletely = stmtResult.bFlowEndCompletely; // 항상 마지막 것으로 업데이트
                    }
                }
                catch (AnalyzerFatalException)
                {
                    bFatal = true;
                }
            }

            if (bFatal)
                throw new AnalyzerFatalException();

            return new StmtResult(new R.BlockStmt(builder.ToImmutable()), bFlowEndCompletely);
        }

        StmtResult AnalyzeBlankStmt()
        {
            return new StmtResult(new R.BlankStmt(), bFlowEndCompletely: false);
        }

        StmtResult AnalyzeExpStmt(S.ExpStmt expStmt)
        {
            var exp = AnalyzeTopLevelExp_Exp(expStmt.Exp, ResolveHint.None, A1301_ExpStmt_ExpressionShouldBeAssignOrCall);

            // TODO: throw가 나오면 bFlowEndCompletely가 
            return new StmtResult(new R.ExpStmt(exp), bFlowEndCompletely: false);
        }

        StmtResult AnalyzeTaskStmt(S.TaskStmt taskStmt)
        {
            var (lambda, args, body) = AnalyzeLambda(globalContext.GetVoidType(), default, taskStmt.Body, taskStmt);
            return new StmtResult(new R.TaskStmt(lambda, args, body), bFlowEndCompletely: false);
        }

        StmtResult AnalyzeAwaitStmt(S.AwaitStmt awaitStmt)
        {                
            var newStmtAnalyzer = NewAnalyzer();

            var bodyResult = newStmtAnalyzer.AnalyzeStmt(awaitStmt.Body);
            return new StmtResult(new R.AwaitStmt(bodyResult.Stmt), bFlowEndCompletely: false);
        }

        StmtResult AnalyzeAsyncStmt(S.AsyncStmt asyncStmt)
        {
            var (lambda, args, body) = AnalyzeLambda(globalContext.GetVoidType(), default, asyncStmt.Body, asyncStmt);
            return new StmtResult(new R.AsyncStmt(lambda, args, body), bFlowEndCompletely: false);
        }

        StmtResult AnalyzeForeachStmt(S.ForeachStmt foreachStmt)
        {
            // SYNTAX: foreach(var elemVarName in iterator) body
            // IR0: foreach(type elemVarName in iteratorLoc)
            var iteratorResult = AnalyzeExp_Loc(foreachStmt.Iterator, ResolveHint.None);
                
            // 먼저, iteratorResult가 anonymous_seq타입인지 확인한다
            if (globalContext.IsSeqType(iteratorResult.TypeSymbol, out var seqItemType)) // seq<>
            {
                var elemType = globalContext.GetSymbolByTypeExp(foreachStmt.Type);
                    
                if (elemType is VarSymbol) // var type처리
                {
                    elemType = seqItemType;
                }
                else
                {
                    // 완전히 같은지 체크
                    if (elemType.Equals(seqItemType))
                    {
                        // relemType = elemType.MakeRPath();
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

                // TODO: ref foreach
                if (foreachStmt.IsRef)
                    throw new NotImplementedException();

                // 루프 컨텍스트에 로컬을 하나 추가하고
                loopAnalyzer.localContext.AddLocalVarInfo(false, elemType, new Name.Normal(foreachStmt.VarName));

                // 본문 분석
                var bodyResult = loopAnalyzer.AnalyzeStmt(foreachStmt.Body);

                var rforeachStmt = new R.ForeachStmt(elemType, foreachStmt.VarName, iteratorResult.Result, bodyResult.Stmt);
                return new StmtResult(rforeachStmt, bodyResult.bFlowEndCompletely);
            }
            else
            {
                // 축약형 처리
                // anonymous_seq를 리턴하는 식으로 변경해준다
                // foreach(var i in [1, 2, 3, 4]) => foreach(var i in [1, 2, 3, 4].GetEnumerator())
                if (globalContext.IsListType(iteratorResult.TypeSymbol, out var listItemType))
                {
                    var elemType = globalContext.GetSymbolByTypeExp(foreachStmt.Type);

                    if (elemType is VarSymbol) // var type처리
                    {
                        elemType = listItemType;
                    }
                    else
                    {
                        // 완전히 같은지 체크
                        if (elemType.Equals(listItemType))
                        {
                            // relemType = elemType.MakeRPath();
                        }
                        else
                        {
                            // 다르다면 seq_cast 시도
                            // foreach(type elemName in seq_cast(type, iteratorLoc))
                            throw new NotImplementedException();
                        }
                    }

                    var listIterator = new R.TempLoc(new R.ListIteratorExp(iteratorResult.Result, globalContext.GetListIterType(listItemType)));

                    // 루프 컨텍스트를 하나 열고
                    var loopAnalyzer = NewAnalyzerWithLoop();

                    // TODO: ref foreach
                    if (foreachStmt.IsRef)
                        throw new NotImplementedException();

                    // 루프 컨텍스트에 로컬을 하나 추가하고
                    loopAnalyzer.localContext.AddLocalVarInfo(false, elemType, new Name.Normal(foreachStmt.VarName));

                    // 본문 분석
                    var bodyResult = loopAnalyzer.AnalyzeStmt(foreachStmt.Body);

                    var rforeachStmt = new R.ForeachStmt(elemType, foreachStmt.VarName, listIterator, bodyResult.Stmt);
                    return new StmtResult(rforeachStmt, bodyResult.bFlowEndCompletely);

                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        StmtResult AnalyzeYieldStmt(S.YieldStmt yieldStmt)
        {
            if (!bodyContext.IsSeqFunc())
                globalContext.AddFatalError(A1401_YieldStmt_YieldShouldBeInSeqFunc, yieldStmt);

            // yield에서는 retType이 명시되는 경우만 있을 것이다
            var callableReturn = bodyContext.GetReturn();
            Debug.Assert(callableReturn != null);

            // NOTICE: 리턴 타입을 힌트로 넣었다
            var retValueExp = AnalyzeExp_Exp(yieldStmt.Value, ResolveHint.Make(callableReturn.Value.Type)); // TODO: ref 처리?
            retValueExp = CastExp_Exp(retValueExp, callableReturn.Value.Type, yieldStmt.Value);

            return new StmtResult(new R.YieldStmt(retValueExp), bFlowEndCompletely: false);
        }            

        StmtResult AnalyzeDirectiveStmt(S.DirectiveStmt directiveStmt)
        {
            switch (directiveStmt.Name)
            {
                case "static_notnull":
                    if (directiveStmt.Args.Length != 1)
                        globalContext.AddFatalError(A2801_StaticNotNullDirective_ShouldHaveOneArgument, directiveStmt);

                    var argResult = AnalyzeExp(directiveStmt.Args[0], ResolveHint.None);                        
                    switch(argResult)
                    {
                        case ExpResult.Loc locResult:
                            return new StmtResult(new R.DirectiveStmt.StaticNotNull(locResult.Result), bFlowEndCompletely: false);

                        default:
                            globalContext.AddFatalError(A2802_StaticNotNullDirective_ArgumentMustBeLocation, directiveStmt);
                            throw new UnreachableCodeException();
                    }

                default:
                    throw new NotImplementedException();
            }
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
                case S.DirectiveStmt directiveStmt: return AnalyzeDirectiveStmt(directiveStmt);
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

        R.Exp AnalyzeTopLevelExp_Exp(S.Exp exp, ResolveHint hint, SyntaxAnalysisErrorCode code)
        {
            if (!IsTopLevelExp(exp))
                globalContext.AddFatalError(code, exp);

            return AnalyzeExp_Exp(exp, hint);
        }

    }

}
