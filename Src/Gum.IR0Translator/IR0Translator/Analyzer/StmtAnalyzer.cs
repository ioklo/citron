using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Gum.Misc;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        [AutoConstructor]
        partial struct StmtResult
        {
            public R.Stmt Stmt { get; }
        }

        // CommandStmt에 있는 expStringElement를 분석한다
        StmtResult AnalyzeCommandStmt(S.CommandStmt cmdStmt)
        {
            var stringExps = new List<R.StringExp>();
            foreach (var cmd in cmdStmt.Commands)
            {
                var expResult = AnalyzeStringExp(cmd);
                stringExps.Add(expResult.Exp);
            }

            return new StmtResult(new R.CommandStmt(stringExps));
        }

        // PrivateGlobalVarDecl이 나오거나, LocalVarDecl이 나오거나
        StmtResult AnalyzeGlobalVarDeclStmt(S.VarDeclStmt varDeclStmt)
        {
            var result = AnalyzeGlobalVarDecl(varDeclStmt.VarDecl);
            return new StmtResult(new R.PrivateGlobalVarDeclStmt(result.Elems));
        }

        StmtResult AnalyzeLocalVarDeclStmt(S.VarDeclStmt varDeclStmt)
        {
            var result = AnalyzeLocalVarDecl(varDeclStmt.VarDecl);
            return new StmtResult(new R.LocalVarDeclStmt(result.VarDecl));
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
            // TODO: if (Type v = exp as Type) 구문 추가

            // if (!AnalyzeExp(ifStmt.Cond, null, out var cond, out var condTypeValue))
            //    return AnalyzeStmtResult.Invalid;

            // if (exp is X) 구문은 exp가 identifier일때만 가능하다
            var idExpCond = ifStmt.Cond as S.IdentifierExp;
            if (idExpCond == null)
                throw new TranslateException(A1001_IfStmt_TestTargetShouldBeVariable, ifStmt.Cond, "if (exp is Type) 구문은 exp가 identifier여야 합니다");

            var typeArgs = GetTypeValues(idExpCond.TypeArgs, context);
            if (!context.GetIdentifierInfo(idExpCond.Value, typeArgs, null, out var idInfo))
                throw new TranslateException(A1002_IfStmt_TestTargetIdentifierNotFound, ifStmt.Cond, $"{idExpCond.Value}를 찾지 못했습니다");

            throw new NotImplementedException();

            //if (idInfo)
            //{
            //    context.AddError(A1001_IfStmt_TestTargetShouldBeVariable, ifStmt.Cond, "if (exp is Type) 구문은 exp가 변수여야 합니다");
            //    return false;
            //}

            //// testTypeValue, 따로 검사 안해도 될것 같다.. 동적 타입 검사
            //// 1. 하위 타입 Base is Derived (Normal)
            //// 2. 인터페이스 Type is Interface (Interface)
            //// 3. enum 하위 타입 Enum is Enum.One (Enum)

            //// TestType이 있을때만 넣는다
            //var testTypeValue = context.GetTypeValueByTypeExp(testTypeExp);

            //if (testTypeValue is EnumElemTypeValue enumElem)
            //{
            //    return AnalyzeIfTestEnumStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, enumElem, out outStmt);                
            //}
            //else if (testTypeValue is NormalTypeValue normal)
            //{
            //    return AnalyzeIfTestClassStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, testTypeValue, out outStmt);
            //}
            //else
            //{
            //    context.AddError(A1003_IfStmt_TestTypeShouldBeEnumOrClass, testTypeExp, "if (exp is Test) 구문은 Test부분이 타입이거나 enum값이어야 합니다");
            //    return false;
            //}
        }

        StmtResult AnalyzeIfStmt(S.IfStmt ifStmt)
        {
            if (ifStmt.TestType != null)
                return AnalyzeIfTestStmt(ifStmt);

            // 순회
            var condResult = AnalyzeExp(ifStmt.Cond, null);
            var bodyResult = AnalyzeStmt(ifStmt.Body);
            StmtResult? elseBodyResult = (ifStmt.ElseBody != null) ? AnalyzeStmt(ifStmt.ElseBody) : null;

            // Analyzer 처리
            if (!context.IsAssignable(context.GetBoolType(), condResult.TypeValue))
                context.AddFatalError(A1004_IfStmt_ConditionShouldBeBool, ifStmt.Cond);

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
                    var varDeclResult = AnalyzeLocalVarDecl(varDeclInit.VarDecl);
                    return new ForStmtInitializerResult(new R.VarDeclForStmtInitializer(varDeclResult.VarDecl));

                case S.ExpForStmtInitializer expInit:
                    var expResult = AnalyzeTopLevelExp(expInit.Exp, null, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall);
                    var expInitType = expResult.TypeValue.GetRType();
                    return new ForStmtInitializerResult(new R.ExpForStmtInitializer(new R.ExpInfo(expResult.Exp, expInitType)));

                default:
                    throw new NotImplementedException();
            }
        }

        StmtResult AnalyzeForStmt(S.ForStmt forStmt)
        {
            var result = context.ExecInLocalScope(() =>
            {
                R.ForStmtInitializer? initializer = null;
                if (forStmt.Initializer != null)
                {
                    var initializerResult = AnalyzeForStmtInitializer(forStmt.Initializer);
                    initializer = initializerResult.Initializer;
                }

                R.Exp? cond = null;
                if (forStmt.CondExp != null)
                {
                    // 밑에서 쓰이므로 분석실패시 종료
                    var condResult = AnalyzeExp(forStmt.CondExp, null);

                    // 에러가 나면 에러를 추가하고 계속 진행
                    if (!context.IsAssignable(context.GetBoolType(), condResult.TypeValue))
                        context.AddError(A1101_ForStmt_ConditionShouldBeBool, forStmt.CondExp);

                    cond = condResult.Exp;
                }

                R.ExpInfo? continueInfo = null;
                if (forStmt.ContinueExp != null)
                {
                    var continueResult = AnalyzeTopLevelExp(forStmt.ContinueExp, null, A1103_ForStmt_ContinueExpShouldBeAssignOrCall);
                    var contExpType = continueResult.TypeValue.GetRType();
                    continueInfo = new R.ExpInfo(continueResult.Exp, contExpType);
                }

                return context.ExecInLoop(() =>
                {
                    var bodyResult = AnalyzeStmt(forStmt.Body);
                    return new R.ForStmt(initializer, cond, continueInfo, bodyResult.Stmt);
                });
            });

            return new StmtResult(result);
        }

        StmtResult AnalyzeContinueStmt(S.ContinueStmt continueStmt)
        {
            if (!context.IsInLoop())
                context.AddFatalError(A1501_ContinueStmt_ShouldUsedInLoop, continueStmt);

            return new StmtResult(R.ContinueStmt.Instance);
        }

        StmtResult AnalyzeBreakStmt(S.BreakStmt breakStmt)
        {
            if (!context.IsInLoop())
            {
                context.AddFatalError(A1601_BreakStmt_ShouldUsedInLoop, breakStmt);
            }

            return new StmtResult(R.BreakStmt.Instance);
        }

        StmtResult AnalyzeReturnStmt(S.ReturnStmt returnStmt)
        {
            // seq 함수는 여기서 모두 처리 
            if (context.IsSeqFunc())
            {
                if (returnStmt.Value != null)
                    context.AddFatalError(A1202_ReturnStmt_SeqFuncShouldReturnVoid, returnStmt);

                return new StmtResult(new R.ReturnStmt(null));
            }

            // 리턴 값이 없을 경우
            if (returnStmt.Value == null)
            {
                var retTypeValue = context.GetRetTypeValue();
                var voidTypeValue = VoidTypeValue.Instance;

                if (retTypeValue == null)
                {
                    context.SetRetTypeValue(voidTypeValue);
                }
                else if (retTypeValue != VoidTypeValue.Instance)
                {
                    context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt);
                }

                return new StmtResult(new R.ReturnStmt(null));
            }
            else
            {
                var retTypeValue = context.GetRetTypeValue();

                // NOTICE: 리턴타입을 힌트로 넣었다
                var valueResult = AnalyzeExp(returnStmt.Value, retTypeValue);

                // 리턴타입이 정해지지 않았을 경우가 있다
                if (retTypeValue == null)
                {
                    context.SetRetTypeValue(valueResult.TypeValue);
                }
                else
                {
                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    if (!context.IsAssignable(retTypeValue, valueResult.TypeValue))
                        context.AddFatalError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt.Value);
                }

                return new StmtResult(new R.ReturnStmt(valueResult.Exp));
            }
        }

        StmtResult AnalyzeBlockStmt(S.BlockStmt blockStmt)
        {
            bool bFatal = false;
            var stmts = new List<R.Stmt>();

            context.ExecInLocalScope(() =>
            {
                foreach (var stmt in blockStmt.Stmts)
                {
                    try
                    {
                        var stmtResult = AnalyzeStmt(stmt);
                        stmts.Add(stmtResult.Stmt);
                    }
                    catch (FatalAnalyzeException)
                    {
                        bFatal = true;
                    }
                }
            });

            if (bFatal)
                throw new FatalAnalyzeException();

            return new StmtResult(new R.BlockStmt(stmts));
        }

        StmtResult AnalyzeBlankStmt()
        {
            return new StmtResult(R.BlankStmt.Instance);
        }

        StmtResult AnalyzeExpStmt(S.ExpStmt expStmt)
        {
            var expResult = AnalyzeTopLevelExp(expStmt.Exp, null, A1301_ExpStmt_ExpressionShouldBeAssignOrCall);

            var expType = expResult.TypeValue.GetRType();
            return new StmtResult(new R.ExpStmt(new R.ExpInfo(expResult.Exp, expType)));
        }

        StmtResult AnalyzeTaskStmt(S.TaskStmt taskStmt)
        {
            var lambdaResult = AnalyzeLambda(taskStmt, taskStmt.Body, ImmutableArray<S.LambdaExpParam>.Empty);
            return new StmtResult(new R.TaskStmt(lambdaResult.Body, lambdaResult.CaptureInfo));
        }

        StmtResult AnalyzeAwaitStmt(S.AwaitStmt awaitStmt)
        {
            var body = context.ExecInLocalScope(() =>
            {
                var bodyResult = AnalyzeStmt(awaitStmt.Body);
                return bodyResult.Stmt;
            });

            return new StmtResult(new R.AwaitStmt(body));
        }

        StmtResult AnalyzeAsyncStmt(S.AsyncStmt asyncStmt)
        {
            var lambdaResult = AnalyzeLambda(asyncStmt, asyncStmt.Body, ImmutableArray<S.LambdaExpParam>.Empty);
            return new StmtResult(new R.AsyncStmt(lambdaResult.Body, lambdaResult.CaptureInfo));
        }

        StmtResult AnalyzeForeachStmt(S.ForeachStmt foreachStmt)
        {
            // iterator
            var iteratorResult = AnalyzeExp(foreachStmt.Iterator, null);
            var elemType = context.GetTypeValueByTypeExp(foreachStmt.Type);

            // TODO: 여기 함수로 빼기
            // 1. List<T>
            // 2. Enumerable<T>, seq 함수일경우
            TypeValue? iteratorElemType = null;
            if (iteratorResult.TypeValue is NormalTypeValue normalIteratorType)
            {
                var typeId = normalIteratorType.GetTypeId();

                if (ModuleInfoEqualityComparer.EqualsItemId(typeId, ItemIds.List))
                {
                    iteratorElemType = normalIteratorType.Entry.TypeArgs[0];
                }
                else if (ModuleInfoEqualityComparer.EqualsItemId(typeId, ItemIds.Enumerable))
                {
                    iteratorElemType = normalIteratorType.Entry.TypeArgs[0];
                }
                else
                {
                    context.AddFatalError(A1801_ForeachStmt_IteratorShouldBeListOrEnumerable, foreachStmt.Iterator);
                }
            }
            else
            {
                context.AddFatalError(A1801_ForeachStmt_IteratorShouldBeListOrEnumerable, foreachStmt.Iterator);
            }

            if (elemType is VarTypeValue) // var 라면, iteratorElemType을 쓴다
            {
                elemType = iteratorElemType;
            }
            else // 아니라면 둘이 호환되는지 확인한다
            {
                // TODO: Cast
                if (!context.IsAssignable(elemType, iteratorElemType))
                    context.AddFatalError(A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType, foreachStmt);
            }

            var stmt = context.ExecInLocalScope(() =>
            {
                context.AddLocalVarInfo(foreachStmt.VarName, elemType);

                return context.ExecInLoop(() =>
                {
                    var bodyResult = AnalyzeStmt(foreachStmt.Body);

                    var elemRType = elemType.GetRType();
                    var iteratorRType = iteratorResult.TypeValue.GetRType();

                    return new R.ForeachStmt(elemRType, foreachStmt.VarName, new R.ExpInfo(iteratorResult.Exp, iteratorRType), bodyResult.Stmt);
                });
            });

            return new StmtResult(stmt);
        }

        StmtResult AnalyzeYieldStmt(S.YieldStmt yieldStmt)
        {
            if (!context.IsSeqFunc())
                context.AddFatalError(A1401_YieldStmt_YieldShouldBeInSeqFunc, yieldStmt);

            // yield에서는 retType이 명시되는 경우만 있을 것이다
            var retTypeValue = context.GetRetTypeValue();
            Debug.Assert(retTypeValue != null);

            // NOTICE: 리턴 타입을 힌트로 넣었다
            var valueResult = AnalyzeExp(yieldStmt.Value, retTypeValue);

            if (!context.IsAssignable(retTypeValue, valueResult.TypeValue))
                context.AddFatalError(A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType, yieldStmt.Value);

            return new StmtResult(new R.YieldStmt(valueResult.Exp));
        }

        StmtResult AnalyzeCommonStmt(S.Stmt stmt)
        {
            switch (stmt)
            {
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

        StmtResult AnalyzeStmt(S.Stmt stmt)
        {
            if (stmt is S.VarDeclStmt varDeclStmt)
                return AnalyzeLocalVarDeclStmt(varDeclStmt);
            else
                return AnalyzeCommonStmt(stmt);
        }
    }
}
