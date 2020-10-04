using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using static Gum.StaticAnalysis.Analyzer;
using static Gum.StaticAnalysis.Analyzer.Misc;

namespace Gum.StaticAnalysis
{
    class StmtAnalyzer
    {
        Analyzer analyzer;

        public StmtAnalyzer(Analyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        // CommandStmt에 있는 expStringElement를 분석한다
        bool AnalyzeCommandStmt(Syntax.CommandStmt cmdStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            bool bResult = true;

            var ir0StrExps = new List<IR0.StringExp>();
            foreach (var cmd in cmdStmt.Commands)
            {
                var ir0StrExpElems = new List<IR0.StringExpElement>();
                foreach (var elem in cmd.Elements)
                {
                    if (!analyzer.AnalyzeStringExpElement(elem, context, out var ir0Elem))
                    {
                        bResult = false;
                        continue;
                    }

                    ir0StrExpElems.Add(ir0Elem);
                }
                
                ir0StrExps.Add(new IR0.StringExp(ir0StrExpElems));
            }

            if (!bResult)
            {
                outStmt = null;
                return false;
            }

            outStmt = new IR0.CommandStmt(ir0StrExps);
            return true;
        }

        // PrivateGlobalVarDecl이 나오거나, LocalVarDecl이 나오거나
        bool AnalyzeVarDeclStmt(Syntax.VarDeclStmt varDeclStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
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
                
                outStmt = new IR0.LocalVarDeclStmt(localVarDecl);
                return true;
            }
        }

        bool AnalyzeIfTestEnumStmt(
            IdentifierInfo.Var varIdInfo,
            IR0.Exp target, 
            Syntax.Stmt thenBody,
            Syntax.Stmt? elseBody,
            TypeValue targetType, TypeValue.EnumElem enumElem, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            bool bResult = true;
            IR0.Stmt? ir0ThenBody = null;
            IR0.Stmt? ir0ElseBody = null;

            context.ExecInLocalScope(() =>
            {
                context.AddOverrideVarInfo(varIdInfo.StorageInfo, targetType);

                if (!AnalyzeStmt(thenBody, context, out ir0ThenBody))
                    bResult = false;

                if (elseBody != null)
                    if (!AnalyzeStmt(elseBody, context, out ir0ElseBody))
                        bResult = false;
            });

            if (bResult != false)
            {
                Debug.Assert(ir0ThenBody != null);
                outStmt = new IR0.IfTestEnumStmt(target, targetType, enumElem.Name, ir0ThenBody, ir0ElseBody);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeIfTestClassStmt(
            IdentifierInfo.Var varIdInfo,
            IR0.Exp target,
            Syntax.Stmt thenBody,
            Syntax.Stmt? elseBody,
            TypeValue targetType,
            TypeValue testType,
            Context context, 
            [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            bool bResult = true;
            IR0.Stmt? ir0ThenBody = null;
            IR0.Stmt? ir0ElseBody = null;

            context.ExecInLocalScope(() =>
            {
                context.AddOverrideVarInfo(varIdInfo.StorageInfo, testType);

                if (!AnalyzeStmt(thenBody, context, out ir0ThenBody))
                    bResult = false;

                if (elseBody != null)
                    if (!AnalyzeStmt(elseBody, context, out ir0ElseBody))
                        bResult = false;
            });

            if (bResult)
            {
                Debug.Assert(ir0ThenBody != null);
                outStmt = new IR0.IfTestClassStmt(target, targetType, testType, ir0ThenBody, ir0ElseBody);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeIfTestStmt(Syntax.IfStmt ifStmt, Syntax.TypeExp testTypeExp, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            outStmt = null;

            // TODO: if (Type v = exp as Type) 구문 추가

            if (!analyzer.AnalyzeExp(ifStmt.Cond, null, context, out var cond, out var condTypeValue))
                return false;

            // if (exp is X) 구문은 exp가 identifier일때만 가능하다
            var idExpCond = ifStmt.Cond as Syntax.IdentifierExp;
            if (idExpCond == null)
            {
                context.ErrorCollector.Add(ifStmt.Cond, "if (exp is Type) 구문은 exp가 identifier여야 합니다");
                return false;
            }

            var typeArgs = GetTypeValues(idExpCond.TypeArgs, context);
            if (!context.GetIdentifierInfo(idExpCond.Value, typeArgs, null, out var idInfo))
            {
                context.ErrorCollector.Add(ifStmt.Cond, $"{idExpCond.Value}를 찾지 못했습니다");
                return false;
            }

            var varIdInfo = idInfo as IdentifierInfo.Var;

            if (varIdInfo == null)
            {
                context.ErrorCollector.Add(ifStmt.Cond, "if (exp is Type) 구문은 exp가 변수여야 합니다");
                return false;
            }

            // testTypeValue, 따로 검사 안해도 될것 같다.. 동적 타입 검사
            // 1. 하위 타입 Base is Derived (Normal)
            // 2. 인터페이스 Type is Interface (Interface)
            // 3. enum 하위 타입 Enum is Enum.One (Enum)

            // TestType이 있을때만 넣는다
            var testTypeValue = context.GetTypeValueByTypeExp(testTypeExp);

            if (testTypeValue is TypeValue.EnumElem enumElem)
            {
                return AnalyzeIfTestEnumStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, enumElem, context, out outStmt);                
            }
            else if (testTypeValue is TypeValue.Normal normal)
            {
                return AnalyzeIfTestClassStmt(varIdInfo, cond, ifStmt.Body, ifStmt.ElseBody, condTypeValue, testTypeValue, context, out outStmt);
            }
            else
            {
                context.ErrorCollector.Add(testTypeExp, "if (exp is Test) 구문은 Test부분이 타입이거나 enum값이어야 합니다");
                return false;
            }
        }

        bool AnalyzeIfStmt(Syntax.IfStmt ifStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt) 
        {
            if (ifStmt.TestType != null)
                return AnalyzeIfTestStmt(ifStmt, ifStmt.TestType, context, out outStmt);

            bool bResult = true;
            var boolTypeValue = analyzer.GetBoolTypeValue();

            if (analyzer.AnalyzeExp(ifStmt.Cond, null, context, out var cond, out var condTypeValue))
            {
                if (!analyzer.IsAssignable(boolTypeValue, condTypeValue, context))
                {
                    context.ErrorCollector.Add(ifStmt, "if 조건 식은 항상 bool형식이어야 합니다");
                    bResult = false;
                }
            }
            else
            {
                bResult = false;
            }

            if (!AnalyzeStmt(ifStmt.Body, context, out var thenBody))
                bResult = false;

            IR0.Stmt? elseBody = null;
            if (ifStmt.ElseBody != null)
                if (!AnalyzeStmt(ifStmt.ElseBody, context, out elseBody))
                    bResult = false;

            if (bResult)
            {
                Debug.Assert(cond != null);
                Debug.Assert(thenBody != null);
                outStmt = new IR0.IfStmt(cond, thenBody, elseBody);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }

        }

        bool AnalyzeForStmtInitializer(Syntax.ForStmtInitializer forInit, Context context, [NotNullWhen(true)] out IR0.ForStmtInitializer? outInitializer)
        {

            switch (forInit)
            {
                case Syntax.VarDeclForStmtInitializer varDeclInit:
                    var builder = new LocalVarDeclBuilder(context);
                    if (analyzer.AnalyzeVarDecl(varDeclInit.VarDecl, builder, context, out var localVarDecl))
                    {
                        outInitializer = new IR0.VarDeclForStmtInitializer(localVarDecl);
                        return true;
                    }

                    outInitializer = null;
                    return false;

                case Syntax.ExpForStmtInitializer expInit:
                    
                    if (analyzer.AnalyzeExp(expInit.Exp, null, context, out var ir0ExpInit, out var expInitType))
                    {
                        outInitializer = new IR0.ExpForStmtInitializer(ir0ExpInit, expInitType);
                        return true;
                    }

                    outInitializer = null;
                    return false;                    

                default: 
                    throw new NotImplementedException();
            }
        }

        bool AnalyzeForStmt(Syntax.ForStmt forStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            bool bResult = true;
            var boolTypeValue = analyzer.GetBoolTypeValue();

            IR0.ForStmtInitializer? initializer = null;
            IR0.Exp? cond = null;
            IR0.ExpAndType? continueInfo = null;
            IR0.Stmt? body = null;

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
                        context.ErrorCollector.Add(forStmt.CondExp, $"{forStmt.CondExp}는 bool 형식이어야 합니다");
                }
                
                if (forStmt.ContinueExp != null)
                {
                    if (analyzer.AnalyzeExp(forStmt.ContinueExp, null, context, out var contExp, out var contExpType))
                    {
                        continueInfo = new IR0.ExpAndType(contExp, contExpType);
                    }
                    else
                    {
                        bResult = false;
                    }
                }

                if (!AnalyzeStmt(forStmt.Body, context, out body))
                    bResult = false;                
            });

            if (bResult)
            {
                // Debug.Assert(ir0Body != null);
                outStmt = new IR0.ForStmt(initializer, cond, continueInfo, body);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeContinueStmt(Syntax.ContinueStmt continueStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            // TODO: loop안에 있는지 확인한다
            outStmt = IR0.ContinueStmt.Instance;
            return true;
        }

        bool AnalyzeBreakStmt(Syntax.BreakStmt breakStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            // loop안에 있는지 확인해야 한다
            outStmt = IR0.BreakStmt.Instance;
            return true;
        }
        
        bool AnalyzeReturnStmt(Syntax.ReturnStmt returnStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            outStmt = null;

            if (returnStmt.Value == null)
            {
                if (!context.IsSeqFunc() && context.GetRetTypeValue() != TypeValue.MakeVoid())
                {
                    context.ErrorCollector.Add(returnStmt.Value!, $"이 함수는 {context.GetRetTypeValue()}을 반환해야 합니다");
                    return false;
                }

                outStmt = new IR0.ReturnStmt(null);
                return true;
            }
            
            if (context.IsSeqFunc())
            {
                context.ErrorCollector.Add(returnStmt, $"seq 함수는 빈 return만 허용됩니다");
                return false;
            }                

            var retTypeValue = context.GetRetTypeValue();

            // NOTICE: 리턴타입을 힌트로 넣었다
            if (!analyzer.AnalyzeExp(returnStmt.Value, retTypeValue, context, out var ir0Value, out var valueType))
                return false;

            if (retTypeValue != null)
            {
                // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                if (!analyzer.IsAssignable(retTypeValue, valueType, context))
                {
                    context.ErrorCollector.Add(returnStmt.Value, $"반환값의 타입 {valueType}는 이 함수의 반환타입과 맞지 않습니다");
                    return false;
                }
            }
            else // 리턴타입이 정해지지 않았을 경우가 있다
            {
                context.SetRetTypeValue(valueType);
            }

            outStmt = new IR0.ReturnStmt(ir0Value);
            return true;
        }

        bool AnalyzeBlockStmt(Syntax.BlockStmt blockStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            bool bResult = true;
            var ir0Stmts = new List<IR0.Stmt>();

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
                outStmt = new IR0.BlockStmt(ir0Stmts);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeExpStmt(Syntax.ExpStmt expStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            bool bResult = true;

            if ((expStmt.Exp is Syntax.UnaryOpExp unOpExp && (unOpExp.Kind != Syntax.UnaryOpKind.PostfixInc || 
                    unOpExp.Kind != Syntax.UnaryOpKind.PostfixDec ||
                    unOpExp.Kind != Syntax.UnaryOpKind.PrefixInc ||
                    unOpExp.Kind != Syntax.UnaryOpKind.PrefixDec)) && 
                (expStmt.Exp is Syntax.BinaryOpExp binOpExp && binOpExp.Kind != Syntax.BinaryOpKind.Assign) &&
                !(expStmt.Exp is Syntax.CallExp) &&
                !(expStmt.Exp is Syntax.MemberCallExp))
            {
                context.ErrorCollector.Add(expStmt, "대입, 함수 호출만 구문으로 사용할 수 있습니다");
                bResult = false;
            }

            if (!analyzer.AnalyzeExp(expStmt.Exp, null, context, out var exp, out var expType))
                bResult = false;

            if( bResult)
            {
                Debug.Assert(exp != null);
                Debug.Assert(expType != null);

                outStmt = new IR0.ExpStmt(exp, expType);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeTaskStmt(Syntax.TaskStmt taskStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            if (!analyzer.AnalyzeLambda(taskStmt.Body, ImmutableArray<Syntax.LambdaExpParam>.Empty, context, out var body, out var captureInfo, out var funcTypeValue))
            {
                outStmt = null;
                return false;
            }

            outStmt = new IR0.TaskStmt(body, captureInfo);
            return true;
        }

        bool AnalyzeAwaitStmt(Syntax.AwaitStmt awaitStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            bool bResult = true;
            IR0.Stmt? body = null;

            context.ExecInLocalScope(() =>
            {
                if (!AnalyzeStmt(awaitStmt.Body, context, out body))
                    bResult = false;
            });

            if (bResult)
            {
                Debug.Assert(body != null);
                outStmt = new IR0.AwaitStmt(body);
                return true;
            }
            else
            {
                outStmt = null;
                return false;
            }
        }

        bool AnalyzeAsyncStmt(Syntax.AsyncStmt asyncStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            if (!analyzer.AnalyzeLambda(asyncStmt.Body, ImmutableArray<Syntax.LambdaExpParam>.Empty, context, out var body, out var captureInfo, out var funcTypeValue))
            {
                outStmt = null;
                return false;
            }

            outStmt = new IR0.AsyncStmt(body, captureInfo);
            return true;
        }
        
        bool AnalyzeForeachStmt(Syntax.ForeachStmt foreachStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            outStmt = null;

            var boolType = analyzer.GetBoolTypeValue();            

            if (!analyzer.AnalyzeExp(foreachStmt.Obj, null, context, out var obj, out var objType))
                return false;

            var elemType = context.GetTypeValueByTypeExp(foreachStmt.Type);

            if (!context.TypeValueService.GetMemberFuncValue(
                objType,
                Name.MakeText("GetEnumerator"), ImmutableArray<TypeValue>.Empty,
                out var getEnumerator))
            {
                context.ErrorCollector.Add(foreachStmt.Obj, "foreach ... in 뒤 객체는 IEnumerator<T> GetEnumerator() 함수가 있어야 합니다.");
                return false;
            }

            // TODO: 일단 인터페이스가 없으므로, bool MoveNext()과 T GetCurrent()가 있는지 본다
            // TODO: 각 함수들이 thiscall인지도 확인해야 한다

            // 1. elemTypeValue가 VarTypeValue이면 GetEnumerator의 리턴값으로 판단한다
            var getEnumeratorType = context.TypeValueService.GetTypeValue(getEnumerator);

            if (!context.TypeValueService.GetMemberFuncValue(
                getEnumeratorType.Return,
                Name.MakeText("MoveNext"), ImmutableArray<TypeValue>.Empty, 
                out var moveNext))
            {
                context.ErrorCollector.Add(foreachStmt.Obj, "enumerator doesn't have 'bool MoveNext()' function");
                return false;
            }

            var moveNextType = context.TypeValueService.GetTypeValue(moveNext);

            if (!analyzer.IsAssignable(boolType, moveNextType.Return, context))
            {
                context.ErrorCollector.Add(foreachStmt.Obj, "enumerator doesn't have 'bool MoveNext()' function");
                return false;
            }

            if (!context.TypeValueService.GetMemberFuncValue(
                getEnumeratorType.Return, 
                Name.MakeText("GetCurrent"), ImmutableArray<TypeValue>.Empty, 
                out var getCurrent))
            {
                context.ErrorCollector.Add(foreachStmt.Obj, "enumerator doesn't have 'GetCurrent()' function");
                return false;
            }

            var getCurrentType = context.TypeValueService.GetTypeValue(getCurrent);
            if (getCurrentType.Return is TypeValue.Void)
            {
                context.ErrorCollector.Add(foreachStmt.Obj, "'GetCurrent()' function cannot return void");
                return false;
            }

            if (elemType is TypeValue.Var)
            {   
                elemType = getCurrentType.Return;

                //var interfaces = typeValueService.GetInterfaces("IEnumerator", 1, funcTypeValue.RetTypeValue);

                //if (1 < interfaces.Count)
                //{
                //    context.ErrorCollector.Add(foreachStmt.Obj, "변수 타입으로 var를 사용하였는데, IEnumerator<T>가 여러개라 어느 것을 사용할지 결정할 수 없습니다.");
                //    return;
                //}
            }
            else
            {
                if (!analyzer.IsAssignable(elemType, getCurrentType.Return, context))
                    context.ErrorCollector.Add(foreachStmt, $"foreach(T ... in obj) 에서 obj.GetEnumerator().GetCurrent()의 결과를 {elemType} 타입으로 캐스팅할 수 없습니다");
            }

            bool bResult = true;
            IR0.Stmt? body = null;

            context.ExecInLocalScope(() =>
            {
                context.AddLocalVarInfo(foreachStmt.VarName, elemType);
                if (!AnalyzeStmt(foreachStmt.Body, context, out body))
                    bResult = false;
            });

            if (!bResult)
            {
                outStmt = null;
                return false;
            }

            Debug.Assert(body != null);
            outStmt = new IR0.ForeachStmt(elemType, foreachStmt.VarName, obj, objType, getEnumeratorType.Return, getEnumerator, moveNext, getCurrent, body);
            return true;
        }

        bool AnalyzeYieldStmt(Syntax.YieldStmt yieldStmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            outStmt = null;

            if (!context.IsSeqFunc())
            {
                context.ErrorCollector.Add(yieldStmt, "seq 함수 내부에서만 yield를 사용할 수 있습니다");
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
                context.ErrorCollector.Add(yieldStmt.Value, $"반환 값의 {valueType} 타입은 이 함수의 반환 타입과 맞지 않습니다");
                return false;
            }

            outStmt = new IR0.YieldStmt(value);
            return true;
        }

        public bool AnalyzeStmt(Syntax.Stmt stmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            switch (stmt)
            {
                case Syntax.CommandStmt cmdStmt: return AnalyzeCommandStmt(cmdStmt, context, out outStmt); 
                case Syntax.VarDeclStmt varDeclStmt: return AnalyzeVarDeclStmt(varDeclStmt, context, out outStmt); 
                case Syntax.IfStmt ifStmt: return AnalyzeIfStmt(ifStmt, context, out outStmt); 
                case Syntax.ForStmt forStmt: return AnalyzeForStmt(forStmt, context, out outStmt); 
                case Syntax.ContinueStmt continueStmt: return AnalyzeContinueStmt(continueStmt, context, out outStmt); 
                case Syntax.BreakStmt breakStmt: return AnalyzeBreakStmt(breakStmt, context, out outStmt); 
                case Syntax.ReturnStmt returnStmt: return AnalyzeReturnStmt(returnStmt, context, out outStmt); 
                case Syntax.BlockStmt blockStmt: return AnalyzeBlockStmt(blockStmt, context, out outStmt); 
                case Syntax.BlankStmt _: outStmt = IR0.BlankStmt.Instance; return true;
                case Syntax.ExpStmt expStmt: return AnalyzeExpStmt(expStmt, context, out outStmt); 
                case Syntax.TaskStmt taskStmt: return AnalyzeTaskStmt(taskStmt, context, out outStmt);
                case Syntax.AwaitStmt awaitStmt: return AnalyzeAwaitStmt(awaitStmt, context, out outStmt); 
                case Syntax.AsyncStmt asyncStmt: return AnalyzeAsyncStmt(asyncStmt, context, out outStmt); 
                case Syntax.ForeachStmt foreachStmt: return AnalyzeForeachStmt(foreachStmt, context, out outStmt); 
                case Syntax.YieldStmt yieldStmt: return AnalyzeYieldStmt(yieldStmt, context, out outStmt); 
                default: throw new NotImplementedException();
            }
        }
    }
}
