using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Citron.Infra;

using Void = Citron.Infra.Void;
using R = Citron.IR0;
using Citron.Analysis;
using Citron.Module;

namespace Citron
{
    partial struct IR0Evaluator
    {
        public static async IAsyncEnumerator<Infra.Void> EvalAsyncEnum(IR0GlobalContext globalContext, IR0EvalContext context, IR0LocalContext localContext, R.Stmt stmt)
        {
            // asyncEnum을 만들기 위해서 내부 함수를 씁니다
            var evaluator = new IR0Evaluator(globalContext, context, localContext);
                 
            await foreach (var _ in evaluator.EvalStmtAsync(stmt))
            {
                yield return Infra.Void.Instance;
            }
        }

        // TODO: CommandProvider가 Parser도 제공해야 할 것 같다
        async ValueTask EvalCommandStmtAsync(R.CommandStmt stmt)
        {
            var tempStr = evalContext.AllocValue<StringValue>(SymbolId.String);

            foreach (var command in stmt.Commands)
            {
                await EvalStringExpAsync(command, tempStr);
                var cmdText = tempStr.GetString();

                await globalContext.ExecuteCommandAsync(cmdText);
            }
        }

        async ValueTask EvalGlobalVarDeclStmtAsync(R.GlobalVarDeclStmt stmt)
        {
            foreach (var elem in stmt.Elems)
            {
                switch(elem)
                {
                    case R.VarDeclElement.Normal normalElem:
                        {
                            var value = evalContext.AllocValue(normalElem.Type);

                            await EvalExpAsync(normalElem.InitExp, value);

                            // 순서 주의, InitExp먼저 실행
                            // TODO: 테스트로 만들기
                            globalContext.AddGlobalVar(normalElem.Name, value);
                            break;
                        }

                    case R.VarDeclElement.Ref refElem:
                        {
                            var refValue = evalContext.AllocRefValue();
                            var target = await EvalLocAsync(refElem.Loc);
                            refValue.SetTarget(target);

                            globalContext.AddGlobalVar(refElem.Name, refValue);
                            break;
                        }

                    default:
                        throw new UnreachableCodeException();
                }
            }
        }

        async ValueTask EvalLocalVarDeclAsync(R.LocalVarDecl localVarDecl)
        {
            foreach (var elem in localVarDecl.Elems)
            {
                switch (elem)
                {
                    case R.VarDeclElement.Normal normalElem:
                        {
                            var value = evalContext.AllocValue(normalElem.Type);

                            await EvalExpAsync(normalElem.InitExp, value);

                            // 순서 주의, TODO: 테스트로 만들기
                            localContext.AddLocalVar(new Name.Normal(normalElem.Name), value);
                            break;
                        }

                    case R.VarDeclElement.Ref refElem:
                        {
                            var refValue = evalContext.AllocRefValue();
                            var target = await EvalLocAsync(refElem.Loc);
                            refValue.SetTarget(target);

                            // 순서 주의, TODO: 테스트로 만들기
                            localContext.AddLocalVar(new Name.Normal(refElem.Name), refValue);
                            break;
                        }

                    default:
                        throw new UnreachableCodeException();
                }
            }
        }


        ValueTask EvalLocalVarDeclStmtAsync(R.LocalVarDeclStmt stmt)
        {
            return EvalLocalVarDeclAsync(stmt.VarDecl);
        }
            
        async IAsyncEnumerable<Void> EvalIfStmtAsync(R.IfStmt stmt)
        {
            var condValue = evalContext.AllocValue<BoolValue>(SymbolId.Bool);
            await EvalExpAsync(stmt.Cond, condValue);

            if (condValue.GetBool())
            {
                await foreach (var _ in EvalStmtAsync(stmt.Body))
                    yield return Void.Instance;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var _ in EvalStmtAsync(stmt.ElseBody))
                        yield return Void.Instance;
            }
        }

        async IAsyncEnumerable<Void> EvalWithNewEnumVarAsync(EnumValue targetValue, string varName, R.Stmt stmt)
        {
            var refValue = evalContext.AllocRefValue();
            refValue.SetTarget(targetValue.GetElemValue());

            localContext.AddLocalVar(new Name.Normal(varName), refValue); // 레퍼런스로 등록

            await foreach (var _ in EvalStmtAsync(stmt))
                yield return Void.Instance;
        }

        IR0Evaluator NewEvaluatorWithNewScope()
        {
            var newLocalContext = new IR0LocalContext(localContext);
            return new IR0Evaluator(globalContext, evalContext, newLocalContext);
        }

        async IAsyncEnumerable<Void> EvalIfTestEnumElemStmtAsync(R.IfTestEnumElemStmt stmt)
        {
            var targetValue = await EvalLocAsync(stmt.Target) as EnumValue;
            if (targetValue == null)
                throw new RuntimeFatalException();

            var bTestPassed = evalContext.IsEnumElem(targetValue, stmt.EnumElem);

            if (bTestPassed)
            {
                if (stmt.VarName != null)
                {
                    var newEvaluator = NewEvaluatorWithNewScope();

                    await foreach (var _ in newEvaluator.EvalWithNewEnumVarAsync(targetValue, stmt.VarName, stmt.Body))
                        yield return Void.Instance;
                }
                else
                {    
                    await foreach (var _ in EvalStmtAsync(stmt.Body))
                        yield return Void.Instance;
                }
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var _ in EvalStmtAsync(stmt.ElseBody))
                        yield return Void.Instance;
            }
        }

        // class B {}, class C<X> : B { }, class D<X> : C<X> { } 의 경우
        // void Func<T>() {
        //     B b = new D<int>();  // b의 겉보기 타입 B, 실제 타입 D<int>
        //     if (b is C<T>) ...   // b의 실제타입 D<int>는 C<T>인가?
        // }
        async IAsyncEnumerable<Void> EvalIfTestClassStmtAsync(R.IfTestClassStmt stmt)
        {
            var targetValue = (ClassValue)await EvalLocAsync(stmt.Target);
            var targetType = targetValue.GetActualType();

            // evalContext 상에서 둘이 같은지 비교해야 한다
            // E |- targetType is stmt.Class
            var bTestPassed = evalContext.IsDerivedClassOf(targetValue, stmt.Class);

            if (bTestPassed)
            {
                await foreach (var _ in EvalStmtAsync(stmt.Body))
                    yield return Void.Instance;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var _ in EvalStmtAsync(stmt.ElseBody))
                        yield return Void.Instance;
            }
        }

        async IAsyncEnumerable<Void> EvalForStmtCoreAsync(R.ForStmt forStmt)
        {
            // continue를 실행시키기 위한 공간은 미리 할당받는다
            if (forStmt.Initializer != null)
            {
                switch (forStmt.Initializer)
                {
                    case R.VarDeclForStmtInitializer varDeclInitializer:
                        await EvalLocalVarDeclAsync(varDeclInitializer.VarDecl);
                        break;

                    case R.ExpForStmtInitializer expInitializer:
                        await EvalExpAsync(expInitializer.Exp, EmptyValue.Instance);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            while (true)
            {
                if (forStmt.CondExp != null)
                {
                    var condValue = evalContext.AllocValue<BoolValue>(SymbolId.Bool);
                    await EvalExpAsync(forStmt.CondExp, condValue);

                    if (!condValue.GetBool())
                        break;
                }

                await foreach (var value in EvalStmtAsync(forStmt.Body))
                    yield return Void.Instance;

                var flowControl = evalContext.GetFlowControl();

                if (flowControl == IR0EvalFlowControl.Break)
                {
                    evalContext.SetFlowControl(IR0EvalFlowControl.None);
                    break;
                }
                else if (flowControl == IR0EvalFlowControl.Continue)
                {
                    evalContext.SetFlowControl(IR0EvalFlowControl.None);
                }
                else if (flowControl == IR0EvalFlowControl.Return)
                {
                    break;
                }
                else
                {
                    Debug.Assert(evalContext.GetFlowControl() == IR0EvalFlowControl.None);
                }

                if (forStmt.ContinueExp != null)
                {
                    await EvalExpAsync(forStmt.ContinueExp, EmptyValue.Instance);
                }
            }
        }

        IAsyncEnumerable<Void> EvalForStmtAsync(R.ForStmt forStmt)
        {
            var newEvaluator = NewEvaluatorWithNewScope();
            return newEvaluator.EvalForStmtCoreAsync(forStmt);
        }

        void EvalContinueStmt(R.ContinueStmt continueStmt)
        {
            evalContext.SetFlowControl(IR0EvalFlowControl.Continue);
        }

        void EvalBreakStmt(R.BreakStmt breakStmt)
        {
            evalContext.SetFlowControl(IR0EvalFlowControl.Break);
        }

        async ValueTask EvalReturnStmtAsync(R.ReturnStmt returnStmt)
        {
            switch (returnStmt.Info)
            {
                case R.ReturnInfo.None:
                    break;

                case R.ReturnInfo.Ref refInfo:
                    {
                        var retValue = (RefValue)evalContext.GetRetValue();
                        var target = await EvalLocAsync(refInfo.Loc);
                        retValue.SetTarget(target);
                        break;
                    }

                case R.ReturnInfo.Expression expInfo:
                    {
                        var retValue = evalContext.GetRetValue();
                        await EvalExpAsync(expInfo.Exp, retValue);
                        break;
                    }
            }

            evalContext.SetFlowControl(IR0EvalFlowControl.Return);
        }

        async IAsyncEnumerable<Void> EvalBlockStmtCoreAsync(R.BlockStmt blockStmt)
        {
            foreach (var stmt in blockStmt.Stmts)
            {
                await foreach (var value in EvalStmtAsync(stmt))
                {
                    yield return Void.Instance;

                    // 확실하지 않아서 걸어둔다
                    Debug.Assert(evalContext.GetFlowControl() == IR0EvalFlowControl.None);
                }

                if (evalContext.GetFlowControl() != IR0EvalFlowControl.None)
                    break;
            }
        }

        IAsyncEnumerable<Void> EvalBlockStmtAsync(R.BlockStmt blockStmt)
        {
            var newEvaluator = NewEvaluatorWithNewScope();
            return newEvaluator.EvalBlockStmtCoreAsync(blockStmt);
        }

        async ValueTask EvalExpStmtAsync(R.ExpStmt expStmt)
        {
            // 할당을 해야하지만, Value기반 구현에서는 할 필요 없다
            await EvalExpAsync(expStmt.Exp, EmptyValue.Instance);
        }            

        async ValueTask EvalTaskStmtAsync(R.TaskStmt taskStmt)
        {
            // 공간 할당
            var lambdaValue = evalContext.AllocValue<LambdaValue>(taskStmt.Lambda.GetSymbolId());

            // 캡쳐
            await EvalCaptureArgs(taskStmt.Lambda, lambdaValue, taskStmt.CaptureArgs);

            // 새로운 컨텍스트             
            var newEvalContext = evalContext.NewLambdaContext(lambdaValue, VoidValue.Instance);
            var newLocalContext = new IR0LocalContext(default, default);
            var newEvaluator = new IR0Evaluator(globalContext, newEvalContext, newLocalContext);

            // 스레드풀에서 실행
            var task = Task.Run(async () =>
            {
                foreach (var stmt in taskStmt.Body)
                    await newEvaluator.EvalStmtSkipYieldAsync(stmt);
            });

            localContext.AddTask(task);
        }

        async IAsyncEnumerable<Void> EvalAwaitStmtAsync(R.AwaitStmt stmt)
        {
            var newLocalContext = localContext.NewTaskLocalContext();
            var newStmtEvaluator = new IR0Evaluator(globalContext, evalContext, newLocalContext);
                
            await foreach (var _ in newStmtEvaluator.EvalStmtAsync(stmt.Body))
                yield return Void.Instance;

            await newLocalContext.WaitAllAsync();
        }

        async ValueTask EvalAsyncStmtAsync(R.AsyncStmt asyncStmt)
        {
            // 공간 할당
            var lambdaValue = evalContext.AllocValue<LambdaValue>(asyncStmt.Lambda.GetSymbolId());

            // 캡쳐
            await EvalCaptureArgs(asyncStmt.Lambda, lambdaValue, asyncStmt.CaptureArgs);

            // 새로운 컨텍스트 
            var newEvalContext = evalContext.NewLambdaContext(lambdaValue, VoidValue.Instance);
            var newLocalContext = new IR0LocalContext(ImmutableDictionary<Name, Value>.Empty, default);
            var newEvaluator = new IR0Evaluator(globalContext, newEvalContext, newLocalContext);

            // 현재 컨텍스트에서 실행
            var taskCompletionSource = new TaskCompletionSource();
            localContext.AddTask(taskCompletionSource.Task);

            foreach (var stmt in asyncStmt.Body)
            {
                await newEvaluator.EvalStmtSkipYieldAsync(stmt);
            }

            taskCompletionSource.SetResult();
        }

        async IAsyncEnumerable<Void> EvalForeachStmtCoreAsync(R.ForeachStmt stmt)
        {
            var iteratorLoc = (SeqValue)await EvalLocAsync(stmt.Iterator);
            var elemValue = evalContext.AllocValue(stmt.ElemType);

            localContext.AddLocalVar(new Name.Normal(stmt.ElemName), elemValue);
            while (await iteratorLoc.NextAsync(elemValue))
            {
                await foreach (var _ in EvalStmtAsync(stmt.Body))
                {
                    yield return Void.Instance;
                }

                var flowControl = evalContext.GetFlowControl();

                if (flowControl == IR0EvalFlowControl.Break)
                {
                    evalContext.SetFlowControl(IR0EvalFlowControl.None);
                    break;
                }
                else if (flowControl == IR0EvalFlowControl.Continue)
                {
                    evalContext.SetFlowControl(IR0EvalFlowControl.None);
                }
                else if (flowControl == IR0EvalFlowControl.Return)
                {
                    break;
                }
                else
                {
                    Debug.Assert(flowControl == IR0EvalFlowControl.None);
                }
            }
        }

        IAsyncEnumerable<Void> EvalForeachStmtAsync(R.ForeachStmt stmt)
        {
            var stmtEvaluator = NewEvaluatorWithNewScope();
            return stmtEvaluator.EvalForeachStmtCoreAsync(stmt);
        }

        async IAsyncEnumerable<Void> EvalYieldStmtAsync(R.YieldStmt yieldStmt)
        {
            await EvalExpAsync(yieldStmt.Value, evalContext.GetYieldValue());
            yield return Void.Instance;
        }

        public IAsyncEnumerable<Void> EvalStmtAsync(R.Stmt stmt)
        {
            static async IAsyncEnumerable<Void> Empty(ValueTask task)
            {
                await task;
                yield break;
            }

            switch (stmt)
            {
                case R.CommandStmt cmdStmt:
                    return Empty(EvalCommandStmtAsync(cmdStmt));                        

                case R.GlobalVarDeclStmt gVarDecl:
                    return Empty(EvalGlobalVarDeclStmtAsync(gVarDecl));

                case R.LocalVarDeclStmt localVarDeclStmt:
                    return Empty(EvalLocalVarDeclStmtAsync(localVarDeclStmt));

                case R.IfStmt ifStmt:
                    return EvalIfStmtAsync(ifStmt);                            

                case R.IfTestClassStmt ifTestClassStmt:
                    return EvalIfTestClassStmtAsync(ifTestClassStmt);

                case R.IfTestEnumElemStmt ifTestEnumStmt:
                    return EvalIfTestEnumElemStmtAsync(ifTestEnumStmt);

                case R.ForStmt forStmt:
                    return EvalForStmtAsync(forStmt);

                case R.ContinueStmt continueStmt:
                    EvalContinueStmt(continueStmt);
                    return AsyncEnumerable.Empty<Void>();

                case R.BreakStmt breakStmt:
                    EvalBreakStmt(breakStmt);
                    return AsyncEnumerable.Empty<Void>();

                case R.ReturnStmt returnStmt:
                    return Empty(EvalReturnStmtAsync(returnStmt)); 

                case R.BlockStmt blockStmt:
                    return EvalBlockStmtAsync(blockStmt);                            

                case R.BlankStmt:
                    return AsyncEnumerable.Empty<Void>();

                case R.ExpStmt expStmt:
                    return Empty(EvalExpStmtAsync(expStmt));

                case R.TaskStmt taskStmt:
                    return Empty(EvalTaskStmtAsync(taskStmt));

                case R.AwaitStmt awaitStmt:
                    return EvalAwaitStmtAsync(awaitStmt);

                case R.AsyncStmt asyncStmt:
                    return Empty(EvalAsyncStmtAsync(asyncStmt));

                case R.ForeachStmt foreachStmt:
                    return EvalForeachStmtAsync(foreachStmt);

                case R.YieldStmt yieldStmt:
                    return EvalYieldStmtAsync(yieldStmt);                            

                default:
                    throw new UnreachableCodeException();
            }
        }

        // yield를 모두 건너뛴다
        public async ValueTask EvalStmtSkipYieldAsync(R.Stmt stmt)
        {
            await foreach (var _ in EvalStmtAsync(stmt)) { }
        }

        // yield를 먹는다
        public async ValueTask EvalBodySkipYieldAsync(ImmutableArray<R.Stmt> body)
        {
            foreach (var stmt in body)
            {
                await foreach (var _ in EvalStmtAsync(stmt)) ;

                if (evalContext.GetFlowControl() == IR0EvalFlowControl.Return)
                    break;
            }
        }
    }
}