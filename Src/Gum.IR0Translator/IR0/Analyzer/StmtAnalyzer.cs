using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using static Gum.IR0.Analyzer;
using static Gum.IR0.Analyzer.Misc;
using static Gum.IR0.AnalyzeErrorCode;

using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Analyzer
    {
        // CommandStmt에 있는 expStringElement를 분석한다
        public CommandStmt AnalyzeCommandStmt(List<StringExp> stringExps)
        {
            return new CommandStmt(stringExps);
        }

        // PrivateGlobalVarDecl이 나오거나, LocalVarDecl이 나오거나
        bool AnalyzeVarDeclStmt(S.VarDeclStmt varDeclStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            outStmt = null;

            if (context.IsGlobalScope())
            {
                var builder = new PrivateGlobalVarDeclStmtBuilder(context);

                if (!AnalyzeVarDecl(varDeclStmt.VarDecl, builder, out var privateGlobalVarDeclStmt))
                    return false;

                outStmt = privateGlobalVarDeclStmt;
                return true;
            }
            else
            {
                var builder = new LocalVarDeclBuilder(context);

                if (!AnalyzeVarDecl(varDeclStmt.VarDecl, builder, out var localVarDecl))
                    return false;
                
                outStmt = new LocalVarDeclStmt(localVarDecl);
                return true;
            }
        }

        //bool AnalyzeIfTestEnumStmt(
        //    IdentifierInfo varIdInfo,
        //    Exp target, 
        //    S.Stmt thenBody,
        //    S.Stmt? elseBody,
        //    TypeValue targetType, TypeValue.EnumElem enumElem, [NotNullWhen(true)] out Stmt? outStmt)
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

        bool AnalyzeIfTestStmt(S.IfStmt ifStmt, S.TypeExp testTypeExp, [NotNullWhen(true)] out Stmt? outStmt)
        {
            outStmt = null;

            // TODO: if (Type v = exp as Type) 구문 추가

            if (!AnalyzeExp(ifStmt.Cond, null, out var cond, out var condTypeValue))
                return false;

            // if (exp is X) 구문은 exp가 identifier일때만 가능하다
            var idExpCond = ifStmt.Cond as S.IdentifierExp;
            if (idExpCond == null)
            {
                context.AddError(A1001_IfStmt_TestTargetShouldBeVariable, ifStmt.Cond, "if (exp is Type) 구문은 exp가 identifier여야 합니다");
                return false;
            }

            var typeArgs = GetTypeValues(idExpCond.TypeArgs, context);
            if (!context.GetIdentifierInfo(idExpCond.Value, typeArgs, null, out var idInfo))
            {
                context.AddError(A1002_IfStmt_TestTargetIdentifierNotFound, ifStmt.Cond, $"{idExpCond.Value}를 찾지 못했습니다");
                return false;
            }

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

            //if (testTypeValue is TypeValue.EnumElem enumElem)
            //{
            //    return AnalyzeIfTestEnumStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, enumElem, out outStmt);                
            //}
            //else if (testTypeValue is TypeValue.Normal normal)
            //{
            //    return AnalyzeIfTestClassStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, testTypeValue, out outStmt);
            //}
            //else
            //{
            //    context.AddError(A1003_IfStmt_TestTypeShouldBeEnumOrClass, testTypeExp, "if (exp is Test) 구문은 Test부분이 타입이거나 enum값이어야 합니다");
            //    return false;
            //}
        }

        bool AnalyzeIfStmt(S.IfStmt ifStmt, [NotNullWhen(true)] out Stmt? outStmt) 
        {
            if (ifStmt.TestType != null)
                return AnalyzeIfTestStmt(ifStmt, ifStmt.TestType, out outStmt);

            bool bResult = true;            

            if (AnalyzeExp(ifStmt.Cond, null, out var cond, out var condTypeValue))
            {
                if (!IsAssignable(TypeValues.Bool, condTypeValue))
                {
                    context.AddError(A1004_IfStmt_ConditionShouldBeBool, ifStmt.Cond, "if 조건 식은 항상 bool형식이어야 합니다");
                    bResult = false;
                }
            }
            else
            {
                bResult = false;
            }

            if (!AnalyzeStmt(ifStmt.Body, out var thenBody))
                bResult = false;

            Stmt? elseBody = null;
            if (ifStmt.ElseBody != null)
                if (!AnalyzeStmt(ifStmt.ElseBody, out elseBody))
                    bResult = false;

            if (bResult)
            {
                Debug.Assert(cond != null);
                Debug.Assert(thenBody != null);
                outStmt = new IfStmt(cond, thenBody, elseBody);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }

        }

        bool AnalyzeForStmtInitializer(S.ForStmtInitializer forInit, [NotNullWhen(true)] out ForStmtInitializer? outInitializer)
        {
            switch (forInit)
            {
                case S.VarDeclForStmtInitializer varDeclInit:
                    var builder = new LocalVarDeclBuilder(context);
                    if (AnalyzeVarDecl(varDeclInit.VarDecl, builder, out var localVarDecl))
                    {
                        outInitializer = new VarDeclForStmtInitializer(localVarDecl);
                        return true;
                    }

                    outInitializer = null;
                    return false;

                case S.ExpForStmtInitializer expInit:                   
                    
                    if (AnalyzeTopLevelExp(expInit.Exp, null, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall, out var ir0ExpInit, out var expInitType))
                    {
                        var expInitTypeId = context.GetType(expInitType);
                        outInitializer = new ExpForStmtInitializer(new ExpInfo(ir0ExpInit, expInitTypeId));
                        return true;
                    }

                    outInitializer = null;
                    return false;                    

                default: 
                    throw new NotImplementedException();
            }
        }

        bool AnalyzeForStmt(S.ForStmt forStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;

            ForStmtInitializer? initializer = null;
            Exp? cond = null;
            ExpInfo? continueInfo = null;
            Stmt? body = null;

            context.ExecInLocalScope(() =>
            {
                if (forStmt.Initializer != null)
                    if (!AnalyzeForStmtInitializer(forStmt.Initializer, out initializer))
                        bResult = false;

                if (forStmt.CondExp != null)
                {
                    // 밑에서 쓰이므로 분석실패시 종료
                    if (!AnalyzeExp(forStmt.CondExp, null, out cond, out var condExpTypeValue))
                    {
                        bResult = false;
                        return;
                    }

                    // 에러가 나면 에러를 추가하고 계속 진행
                    if (!IsAssignable(TypeValues.Bool, condExpTypeValue))
                        context.AddError(A1101_ForStmt_ConditionShouldBeBool, forStmt.CondExp, $"{forStmt.CondExp}는 bool 형식이어야 합니다");
                }
                
                if (forStmt.ContinueExp != null)
                {
                    if (AnalyzeTopLevelExp(forStmt.ContinueExp, null, A1103_ForStmt_ContinueExpShouldBeAssignOrCall, out var contExp, out var contExpType))
                    {
                        var contExpTypeId = context.GetType(contExpType);
                        continueInfo = new ExpInfo(contExp, contExpTypeId);
                    }
                    else
                    {
                        bResult = false;
                    }
                }

                context.ExecInLoop(() =>
                {
                    if (!AnalyzeStmt(forStmt.Body, out body))
                        bResult = false;
                });
            });

            if (bResult)
            {
                Debug.Assert(body != null);
                outStmt = new ForStmt(initializer, cond, continueInfo, body);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeContinueStmt(S.ContinueStmt continueStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            if (!context.IsInLoop())
            {
                outStmt = null;
                context.AddError(A1501_ContinueStmt_ShouldUsedInLoop, continueStmt, "continue는 루프 안에서만 사용할 수 있습니다");
                return false;
            }
            
            outStmt = ContinueStmt.Instance;
            return true;
        }

        bool AnalyzeBreakStmt(S.BreakStmt breakStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            if (!context.IsInLoop())
            {
                outStmt = null;
                context.AddError(A1601_BreakStmt_ShouldUsedInLoop, breakStmt, "break는 루프 안에서만 사용할 수 있습니다");
                return false;
            }

            outStmt = BreakStmt.Instance;
            return true;
        }
        
        bool AnalyzeReturnStmt(S.ReturnStmt returnStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            outStmt = null;

            // seq 함수는 여기서 모두 처리 
            if (context.IsSeqFunc())
            {
                if (returnStmt.Value != null)
                {
                    context.AddError(A1202_ReturnStmt_SeqFuncShouldReturnVoid, returnStmt, $"seq 함수는 빈 return만 허용됩니다");
                    return false;
                }

                outStmt = new ReturnStmt(null);
                return true;
            }           

            // 리턴 값이 없을 경우
            if (returnStmt.Value == null)
            {
                var retTypeValue = context.GetRetTypeValue();
                var voidTypeValue = TypeValue.Void.Instance;

                if (retTypeValue == null)
                {
                    context.SetRetTypeValue(voidTypeValue);
                }
                else if (retTypeValue != TypeValue.Void.Instance)
                {
                    context.AddError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt, $"이 함수는 {context.GetRetTypeValue()}을 반환해야 합니다");
                    return false;
                }

                outStmt = new ReturnStmt(null);
                return true;
            }
            else
            {
                var retTypeValue = context.GetRetTypeValue();

                // NOTICE: 리턴타입을 힌트로 넣었다
                if (!AnalyzeExp(returnStmt.Value, retTypeValue, out var ir0Value, out var valueType))
                    return false;

                // 리턴타입이 정해지지 않았을 경우가 있다
                if (retTypeValue == null)
                {
                    context.SetRetTypeValue(valueType);
                }
                else 
                {
                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    if (!IsAssignable(retTypeValue, valueType))
                    {
                        context.AddError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt.Value, $"반환값의 타입 {valueType}는 이 함수의 반환타입과 맞지 않습니다");
                        return false;
                    }
                }

                outStmt = new ReturnStmt(ir0Value);
                return true;
            }
        }

        bool AnalyzeBlockStmt(S.BlockStmt blockStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;
            var ir0Stmts = new List<Stmt>();

            context.ExecInLocalScope(() =>
            {
                foreach (var stmt in blockStmt.Stmts)
                {
                    if (!AnalyzeStmt(stmt, out var ir0Stmt))
                    {
                        bResult = false;
                    }
                    else
                    {
                        ir0Stmts.Add(ir0Stmt);
                    }
                }
            });

            if (bResult)
            {
                outStmt = new BlockStmt(ir0Stmts);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }       

        bool AnalyzeExpStmt(S.ExpStmt expStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;
            
            if (!AnalyzeTopLevelExp(expStmt.Exp, null, A1301_ExpStmt_ExpressionShouldBeAssignOrCall, out var exp, out var expType))
                bResult = false;

            if (bResult)
            {
                Debug.Assert(exp != null);
                Debug.Assert(expType != null);

                var expTypeId = context.GetType(expType);
                outStmt = new ExpStmt(new ExpInfo(exp, expTypeId));
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeTaskStmt(S.TaskStmt taskStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            if (!AnalyzeLambda(taskStmt, taskStmt.Body, ImmutableArray<S.LambdaExpParam>.Empty, out var body, out var captureInfo, out var _))
            {
                outStmt = null;
                return false;
            }

            outStmt = new TaskStmt(body, captureInfo);
            return true;
        }

        bool AnalyzeAwaitStmt(S.AwaitStmt awaitStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;
            Stmt? body = null;

            context.ExecInLocalScope(() =>
            {
                if (!AnalyzeStmt(awaitStmt.Body, out body))
                    bResult = false;
            });

            if (bResult)
            {
                Debug.Assert(body != null);
                outStmt = new AwaitStmt(body);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeAsyncStmt(S.AsyncStmt asyncStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            if (!AnalyzeLambda(asyncStmt, asyncStmt.Body, ImmutableArray<S.LambdaExpParam>.Empty, out var body, out var captureInfo, out var _))
            {
                outStmt = null;
                return false;
            }

            outStmt = new AsyncStmt(body, captureInfo);
            return true;
        }        
        
        bool AnalyzeForeachStmt(S.ForeachStmt foreachStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            outStmt = null;

            // iterator
            if (!AnalyzeExp(foreachStmt.Iterator, null, out var iterator, out var iteratorType))
                return false;

            var elemType = context.GetTypeValueByTypeExp(foreachStmt.Type);

            // TODO: 여기 함수로 빼기
            // 1. List<T>
            // 2. Enumerable<T>, seq 함수일경우
            TypeValue iteratorElemType;
            if (iteratorType is TypeValue.Normal normalIteratorType)
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
                    context.AddError(A1801_ForeachStmt_IteratorShouldBeListOrEnumerable, foreachStmt.Iterator, "foreach의 반복자 부분은 List<>, Enumerable<>타입이어야 합니다");
                    return false;
                }
            }
            else
            {
                context.AddError(A1801_ForeachStmt_IteratorShouldBeListOrEnumerable, foreachStmt.Iterator, "foreach의 반복자 부분은 List<>, Enumerable<>타입이어야 합니다");
                return false;
            }
            
            if (elemType is TypeValue.Var) // var 라면, iteratorElemType을 쓴다
            {
                elemType = iteratorElemType;
            }
            else // 아니라면 둘이 호환되는지 확인한다
            {
                // TODO: Cast
                if (!IsAssignable(elemType, iteratorElemType))
                {
                    context.AddError(A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType, foreachStmt, "foreach 변수에 반복자 원소를 대입 할 수 없습니다");
                    return false;
                }
            }

            // varname
            // body

            Stmt? body = null;
            context.ExecInLocalScope(() =>
            {
                context.AddLocalVarInfo(foreachStmt.VarName, elemType);

                context.ExecInLoop(() =>
                {
                    AnalyzeStmt(foreachStmt.Body, out body);                        
                });
            });

            if (body == null)
            {
                outStmt = null;
                return false;
            }

            var elemTypeId = context.GetType(elemType);
            var iteratorTypeId = context.GetType(iteratorType);
            outStmt = new ForeachStmt(elemTypeId, foreachStmt.VarName, new ExpInfo(iterator, iteratorTypeId), body);
            return true;
        }

        bool AnalyzeYieldStmt(S.YieldStmt yieldStmt, [NotNullWhen(true)] out Stmt? outStmt)
        {
            outStmt = null;

            if (!context.IsSeqFunc())
            {
                context.AddError(A1401_YieldStmt_YieldShouldBeInSeqFunc, yieldStmt, "seq 함수 내부에서만 yield를 사용할 수 있습니다");
                return false;
            }            

            // yield에서는 retType이 명시되는 경우만 있을 것이다
            var retTypeValue = context.GetRetTypeValue();
            Debug.Assert(retTypeValue != null);

            // NOTICE: 리턴 타입을 힌트로 넣었다
            if (!AnalyzeExp(yieldStmt.Value, retTypeValue, out var value, out var valueType))
                return false;

            if (!IsAssignable(retTypeValue, valueType))
            {
                context.AddError(A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType, yieldStmt.Value, $"반환 값의 {valueType} 타입은 이 함수의 반환 타입과 맞지 않습니다");
                return false;
            }

            outStmt = new YieldStmt(value);
            return true;
        }        
    }
}
