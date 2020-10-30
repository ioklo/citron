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
    class StmtAnalyzer
    {
        Analyzer analyzer;

        public StmtAnalyzer(Analyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        // CommandStmt에 있는 expStringElement를 분석한다
        bool AnalyzeCommandStmt(S.CommandStmt cmdStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;

            var ir0StrExps = new List<StringExp>();
            foreach (var cmd in cmdStmt.Commands)
            {
                var ir0StrExpElems = new List<StringExpElement>();
                foreach (var elem in cmd.Elements)
                {
                    if (!analyzer.AnalyzeStringExpElement(elem, context, out var ir0Elem))
                    {
                        bResult = false;
                        continue;
                    }

                    ir0StrExpElems.Add(ir0Elem);
                }
                
                ir0StrExps.Add(new StringExp(ir0StrExpElems));
            }

            if (!bResult)
            {
                outStmt = null;
                return false;
            }

            outStmt = new CommandStmt(ir0StrExps);
            return true;
        }

        // PrivateGlobalVarDecl이 나오거나, LocalVarDecl이 나오거나
        bool AnalyzeVarDeclStmt(S.VarDeclStmt varDeclStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            outStmt = null;

            if (context.IsGlobalScope())
            {
                var builder = new PrivateGlobalVarDeclStmtBuilder(context);

                if (!analyzer.AnalyzeVarDecl(varDeclStmt.VarDecl, builder, context, out var privateGlobalVarDeclStmt))
                    return false;

                outStmt = privateGlobalVarDeclStmt;
                return true;
            }
            else
            {
                var builder = new LocalVarDeclBuilder(context);

                if (!analyzer.AnalyzeVarDecl(varDeclStmt.VarDecl, builder, context, out var localVarDecl))
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
        //    TypeValue targetType, TypeValue.EnumElem enumElem, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        //{
        //    bool bResult = true;
        //    Stmt? ir0ThenBody = null;
        //    Stmt? ir0ElseBody = null;

        //    context.ExecInLocalScope(() =>
        //    {
        //        context.AddOverrideVarInfo(varIdInfo.StorageInfo, targetType);

        //        if (!AnalyzeStmt(thenBody, context, out ir0ThenBody))
        //            bResult = false;

        //        if (elseBody != null)
        //            if (!AnalyzeStmt(elseBody, context, out ir0ElseBody))
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

        //        if (!AnalyzeStmt(thenBody, context, out ir0ThenBody))
        //            bResult = false;

        //        if (elseBody != null)
        //            if (!AnalyzeStmt(elseBody, context, out ir0ElseBody))
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

        bool AnalyzeIfTestStmt(S.IfStmt ifStmt, S.TypeExp testTypeExp, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            outStmt = null;

            // TODO: if (Type v = exp as Type) 구문 추가

            if (!analyzer.AnalyzeExp(ifStmt.Cond, null, context, out var cond, out var condTypeValue))
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
            //    return AnalyzeIfTestEnumStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, enumElem, context, out outStmt);                
            //}
            //else if (testTypeValue is TypeValue.Normal normal)
            //{
            //    return AnalyzeIfTestClassStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, testTypeValue, context, out outStmt);
            //}
            //else
            //{
            //    context.AddError(A1003_IfStmt_TestTypeShouldBeEnumOrClass, testTypeExp, "if (exp is Test) 구문은 Test부분이 타입이거나 enum값이어야 합니다");
            //    return false;
            //}
        }

        bool AnalyzeIfStmt(S.IfStmt ifStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt) 
        {
            if (ifStmt.TestType != null)
                return AnalyzeIfTestStmt(ifStmt, ifStmt.TestType, context, out outStmt);

            bool bResult = true;
            var boolTypeValue = analyzer.GetBoolTypeValue();

            if (analyzer.AnalyzeExp(ifStmt.Cond, null, context, out var cond, out var condTypeValue))
            {
                if (!analyzer.IsAssignable(boolTypeValue, condTypeValue, context))
                {
                    context.AddError(A1004_IfStmt_ConditionShouldBeBool, ifStmt.Cond, "if 조건 식은 항상 bool형식이어야 합니다");
                    bResult = false;
                }
            }
            else
            {
                bResult = false;
            }

            if (!AnalyzeStmt(ifStmt.Body, context, out var thenBody))
                bResult = false;

            Stmt? elseBody = null;
            if (ifStmt.ElseBody != null)
                if (!AnalyzeStmt(ifStmt.ElseBody, context, out elseBody))
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

        bool AnalyzeForStmtInitializer(S.ForStmtInitializer forInit, Context context, [NotNullWhen(true)] out ForStmtInitializer? outInitializer)
        {
            switch (forInit)
            {
                case S.VarDeclForStmtInitializer varDeclInit:
                    var builder = new LocalVarDeclBuilder(context);
                    if (analyzer.AnalyzeVarDecl(varDeclInit.VarDecl, builder, context, out var localVarDecl))
                    {
                        outInitializer = new VarDeclForStmtInitializer(localVarDecl);
                        return true;
                    }

                    outInitializer = null;
                    return false;

                case S.ExpForStmtInitializer expInit:                   
                    
                    if (analyzer.AnalyzeTopLevelExp(expInit.Exp, null, A1102_ForStmt_ExpInitializerShouldBeAssignOrCall, context, out var ir0ExpInit, out var expInitType))
                    {
                        var expInitTypeId = context.GetTypeId(expInitType);
                        outInitializer = new ExpForStmtInitializer(new ExpInfo(ir0ExpInit, expInitTypeId));
                        return true;
                    }

                    outInitializer = null;
                    return false;                    

                default: 
                    throw new NotImplementedException();
            }
        }

        bool AnalyzeForStmt(S.ForStmt forStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;
            var boolTypeValue = analyzer.GetBoolTypeValue();

            ForStmtInitializer? initializer = null;
            Exp? cond = null;
            ExpInfo? continueInfo = null;
            Stmt? body = null;

            context.ExecInLocalScope(() =>
            {
                if (forStmt.Initializer != null)
                    if (!AnalyzeForStmtInitializer(forStmt.Initializer, context, out initializer))
                        bResult = false;

                if (forStmt.CondExp != null)
                {
                    // 밑에서 쓰이므로 분석실패시 종료
                    if (!analyzer.AnalyzeExp(forStmt.CondExp, null, context, out cond, out var condExpTypeValue))
                    {
                        bResult = false;
                        return;
                    }

                    // 에러가 나면 에러를 추가하고 계속 진행
                    if (!analyzer.IsAssignable(boolTypeValue, condExpTypeValue, context))
                        context.AddError(A1101_ForStmt_ConditionShouldBeBool, forStmt.CondExp, $"{forStmt.CondExp}는 bool 형식이어야 합니다");
                }
                
                if (forStmt.ContinueExp != null)
                {
                    if (analyzer.AnalyzeTopLevelExp(forStmt.ContinueExp, null, A1103_ForStmt_ContinueExpShouldBeAssignOrCall, context, out var contExp, out var contExpType))
                    {
                        var contExpTypeId = context.GetTypeId(contExpType);
                        continueInfo = new ExpInfo(contExp, contExpTypeId);
                    }
                    else
                    {
                        bResult = false;
                    }
                }

                context.ExecInLoop(() =>
                {
                    if (!AnalyzeStmt(forStmt.Body, context, out body))
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

        bool AnalyzeContinueStmt(S.ContinueStmt continueStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
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

        bool AnalyzeBreakStmt(S.BreakStmt breakStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
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
        
        bool AnalyzeReturnStmt(S.ReturnStmt returnStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
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
                var voidTypeValue = TypeValue.MakeVoid();

                if (retTypeValue == null)
                {
                    context.SetRetTypeValue(voidTypeValue);
                }
                else if (retTypeValue != TypeValue.MakeVoid())
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
                if (!analyzer.AnalyzeExp(returnStmt.Value, retTypeValue, context, out var ir0Value, out var valueType))
                    return false;

                // 리턴타입이 정해지지 않았을 경우가 있다
                if (retTypeValue == null)
                {
                    context.SetRetTypeValue(valueType);
                }
                else 
                {
                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    if (!analyzer.IsAssignable(retTypeValue, valueType, context))
                    {
                        context.AddError(A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType, returnStmt.Value, $"반환값의 타입 {valueType}는 이 함수의 반환타입과 맞지 않습니다");
                        return false;
                    }
                }

                outStmt = new ReturnStmt(ir0Value);
                return true;
            }
        }

        bool AnalyzeBlockStmt(S.BlockStmt blockStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;
            var ir0Stmts = new List<Stmt>();

            context.ExecInLocalScope(() =>
            {
                foreach (var stmt in blockStmt.Stmts)
                {
                    if (!AnalyzeStmt(stmt, context, out var ir0Stmt))
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

        bool AnalyzeExpStmt(S.ExpStmt expStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;
            
            if (!analyzer.AnalyzeTopLevelExp(expStmt.Exp, null, A1301_ExpStmt_ExpressionShouldBeAssignOrCall, context, out var exp, out var expType))
                bResult = false;

            if (bResult)
            {
                Debug.Assert(exp != null);
                Debug.Assert(expType != null);

                var expTypeId = context.GetTypeId(expType);
                outStmt = new ExpStmt(new ExpInfo(exp, expTypeId));
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeTaskStmt(S.TaskStmt taskStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            if (!analyzer.AnalyzeLambda(taskStmt, taskStmt.Body, ImmutableArray<S.LambdaExpParam>.Empty, context, out var body, out var captureInfo, out var _))
            {
                outStmt = null;
                return false;
            }

            outStmt = new TaskStmt(body, captureInfo);
            return true;
        }

        bool AnalyzeAwaitStmt(S.AwaitStmt awaitStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            bool bResult = true;
            Stmt? body = null;

            context.ExecInLocalScope(() =>
            {
                if (!AnalyzeStmt(awaitStmt.Body, context, out body))
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

        bool AnalyzeAsyncStmt(S.AsyncStmt asyncStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            if (!analyzer.AnalyzeLambda(asyncStmt, asyncStmt.Body, ImmutableArray<S.LambdaExpParam>.Empty, context, out var body, out var captureInfo, out var _))
            {
                outStmt = null;
                return false;
            }

            outStmt = new AsyncStmt(body, captureInfo);
            return true;
        }        
        
        bool AnalyzeForeachStmt(S.ForeachStmt foreachStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            outStmt = null;

            if (!analyzer.AnalyzeExp(foreachStmt.Obj, null, context, out var obj, out var objType))
                return false;

            // in에 오는 것들은 
            throw new NotImplementedException();

            //var elemType = context.GetTypeValueByTypeExp(foreachStmt.Type);

            //if (!context.TypeValueService.GetMemberFuncValue(
            //    objType,
            //    Name.MakeText("GetEnumerator"), ImmutableArray<TypeValue>.Empty,
            //    out var getEnumerator))
            //{
            //    context.AddError(, foreachStmt.Obj, "foreach ... in 뒤 객체는 IEnumerator<T> GetEnumerator() 함수가 있어야 합니다.");
            //    return false;
            //}

            //// TODO: 일단 인터페이스가 없으므로, bool MoveNext()과 T GetCurrent()가 있는지 본다
            //// TODO: 각 함수들이 thiscall인지도 확인해야 한다            
            //if (elemType is TypeValue.Var)
            //{
            //    elemType = getCurrentType.Return;

            //    //var interfaces = typeValueService.GetInterfaces("IEnumerator", 1, funcTypeValue.RetTypeValue);

            //    //if (1 < interfaces.Count)
            //    //{
            //    //    context.ErrorCollector.Add(foreachStmt.Obj, "변수 타입으로 var를 사용하였는데, IEnumerator<T>가 여러개라 어느 것을 사용할지 결정할 수 없습니다.");
            //    //    return;
            //    //}
            //}
            //else
            //{
            //    if (!analyzer.IsAssignable(elemType, getCurrentType.Return, context))
            //        context.ErrorCollector.Add(foreachStmt, $"foreach(T ... in obj) 에서 obj.GetEnumerator().GetCurrent()의 결과를 {elemType} 타입으로 캐스팅할 수 없습니다");
            //}

            //bool bResult = true;
            //Stmt? body = null;

            //context.ExecInLocalScope(() =>
            //{
            //    context.AddLocalVarInfo(foreachStmt.VarName, elemType);
            //    if (!AnalyzeStmt(foreachStmt.Body, context, out body))
            //        bResult = false;
            //});

            //if (!bResult)
            //{
            //    outStmt = null;
            //    return false;
            //}

            //Debug.Assert(body != null);
            //var elemTypeId = context.GetTypeId(elemType);
            //var objTypeId = context.GetTypeId(objType);
            //outStmt = new ForeachStmt(elemTypeId, foreachStmt.VarName, new ExpInfo(obj, objTypeId), body);
            //return true;
        }

        bool AnalyzeYieldStmt(S.YieldStmt yieldStmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
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
            if (!analyzer.AnalyzeExp(yieldStmt.Value, retTypeValue, context, out var value, out var valueType))
                return false;

            if (!analyzer.IsAssignable(retTypeValue, valueType, context))
            {
                context.AddError(A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType, yieldStmt.Value, $"반환 값의 {valueType} 타입은 이 함수의 반환 타입과 맞지 않습니다");
                return false;
            }

            outStmt = new YieldStmt(value);
            return true;
        }

        public bool AnalyzeStmt(S.Stmt stmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            switch (stmt)
            {
                case S.CommandStmt cmdStmt: return AnalyzeCommandStmt(cmdStmt, context, out outStmt); 
                case S.VarDeclStmt varDeclStmt: return AnalyzeVarDeclStmt(varDeclStmt, context, out outStmt); 
                case S.IfStmt ifStmt: return AnalyzeIfStmt(ifStmt, context, out outStmt); 
                case S.ForStmt forStmt: return AnalyzeForStmt(forStmt, context, out outStmt); 
                case S.ContinueStmt continueStmt: return AnalyzeContinueStmt(continueStmt, context, out outStmt); 
                case S.BreakStmt breakStmt: return AnalyzeBreakStmt(breakStmt, context, out outStmt); 
                case S.ReturnStmt returnStmt: return AnalyzeReturnStmt(returnStmt, context, out outStmt); 
                case S.BlockStmt blockStmt: return AnalyzeBlockStmt(blockStmt, context, out outStmt); 
                case S.BlankStmt _: outStmt = BlankStmt.Instance; return true;
                case S.ExpStmt expStmt: return AnalyzeExpStmt(expStmt, context, out outStmt); 
                case S.TaskStmt taskStmt: return AnalyzeTaskStmt(taskStmt, context, out outStmt);
                case S.AwaitStmt awaitStmt: return AnalyzeAwaitStmt(awaitStmt, context, out outStmt); 
                case S.AsyncStmt asyncStmt: return AnalyzeAsyncStmt(asyncStmt, context, out outStmt); 
                case S.ForeachStmt foreachStmt: return AnalyzeForeachStmt(foreachStmt, context, out outStmt); 
                case S.YieldStmt yieldStmt: return AnalyzeYieldStmt(yieldStmt, context, out outStmt); 
                default: throw new NotImplementedException();
            }
        }
    }
}
