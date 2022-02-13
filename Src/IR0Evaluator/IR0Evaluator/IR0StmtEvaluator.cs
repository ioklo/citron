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

namespace Citron.IR0Evaluator
{
    struct IR0StmtEvaluator
    {
        IR0GlobalContext globalContext;
        IR0EvalContext context;
        LocalContext localContext;
        LocalTaskContext localTaskContext;

        // yield를 먹는다
        public static async ValueTask EvalTopLevelStmtsAsync(IR0GlobalContext globalContext, IR0EvalContext context, LocalContext localContext, LocalTaskContext localTaskContext, ImmutableArray<R.Stmt> topLevelStmts)
        {
            var stmtEvaluator = new IR0StmtEvaluator(globalContext, context, localContext, localTaskContext);

            foreach (var topLevelStmt in topLevelStmts)
            {
                await foreach (var _ in stmtEvaluator.EvalStmtAsync(topLevelStmt));

                if (context.GetFlowControl() == EvalFlowControl.Return)
                    break;
            }
        }

        // yield를 먹는다
        public static async ValueTask EvalAsync(IR0GlobalContext globalContext, IR0EvalContext context, LocalContext localContext, LocalTaskContext localTaskContext, R.Stmt stmt)
        {
            var stmtEvaluator = new IR0StmtEvaluator(globalContext, context, localContext, localTaskContext);
            await foreach (var _ in stmtEvaluator.EvalStmtAsync(stmt)) ;
        }

        public static async ValueTask EvalConstructorAsync(
            IR0GlobalContext globalContext, 
            ImmutableDictionary<R.Name, Value> args, 
            Value thisValue, 
            (ConstructorRuntimeItem BaseConstructorItem, ImmutableArray<R.Argument> Args)? baseCallInfo,
            R.Stmt body)
        {
            var context = new IR0EvalContext(default, EvalFlowControl.None, thisValue, VoidValue.Instance);
            var localContext = new LocalContext(args);
            var localTaskContext = new LocalTaskContext();

            if (baseCallInfo != null)
            {
                var baseCallArgs = await IR0ExpEvaluator.EvalArgumentsAsync(globalContext, context, localContext, baseCallInfo.Value.BaseConstructorItem.Parameters, baseCallInfo.Value.Args);
                await baseCallInfo.Value.BaseConstructorItem.InvokeAsync(thisValue, baseCallArgs);
            }

            await EvalAsync(globalContext, context, localContext, localTaskContext, body);
        }

        public static async IAsyncEnumerator<Infra.Void> EvalAsyncEnum(IR0GlobalContext globalContext, IR0EvalContext context, LocalContext localContext, LocalTaskContext localTaskContext, R.Stmt stmt)
        {
            // asyncEnum을 만들기 위해서 내부 함수를 씁니다
            var evaluator = new IR0StmtEvaluator(globalContext, context, localContext, localTaskContext);
                 
            await foreach (var _ in evaluator.EvalStmtAsync(stmt))
            {
                yield return Infra.Void.Instance;
            }
        }

        ValueTask EvalExpAsync(R.Exp exp, Value result) => IR0ExpEvaluator.EvalAsync(globalContext, context, localContext, exp, result);
        ValueTask<Value> EvalLocAsync(R.Loc loc) => IR0LocEvaluator.EvalAsync(globalContext, context, localContext, loc);

        IR0StmtEvaluator(IR0GlobalContext globalContext, IR0EvalContext context, LocalContext localContext, LocalTaskContext localTaskContext)
        {
            this.globalContext = globalContext;
            this.context = context;
            this.localContext = localContext;
            this.localTaskContext = localTaskContext;
        }

        // TODO: CommandProvider가 Parser도 제공해야 할 것 같다
        async ValueTask EvalCommandStmtAsync(R.CommandStmt stmt)
        {
            var tempStr = context.AllocValue<StringValue>(R.Path.String);

            foreach (var command in stmt.Commands)
            {
                await IR0ExpEvaluator.EvalStringExpAsync(globalContext, context, localContext, command, tempStr);
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
                            var value = context.AllocValue(normalElem.Type);

                            await EvalExpAsync(normalElem.InitExp, value);

                            // 순서 주의, InitExp먼저 실행
                            // TODO: 테스트로 만들기
                            globalContext.AddGlobalVar(normalElem.Name, value);
                            break;
                        }

                    case R.VarDeclElement.Ref refElem:
                        {
                            var refValue = globalContext.AllocRefValue();
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
                            var value = context.AllocValue(normalElem.Type);

                            await EvalExpAsync(normalElem.InitExp, value);

                            // 순서 주의, TODO: 테스트로 만들기
                            localContext.AddLocalVar(new R.Name.Normal(normalElem.Name), value);
                            break;
                        }

                    case R.VarDeclElement.Ref refElem:
                        {
                            var refValue = globalContext.AllocRefValue();
                            var target = await EvalLocAsync(refElem.Loc);
                            refValue.SetTarget(target);

                            // 순서 주의, TODO: 테스트로 만들기
                            localContext.AddLocalVar(new R.Name.Normal(refElem.Name), refValue);
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
            var condValue = context.AllocValue<BoolValue>(R.Path.Bool);
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
            var refValue = globalContext.AllocRefValue();
            refValue.SetTarget(targetValue.GetElemValue());

            localContext.AddLocalVar(new R.Name.Normal(varName), refValue); // 레퍼런스로 등록

            await foreach (var _ in EvalStmtAsync(stmt))
                yield return Void.Instance;
        }

        IR0StmtEvaluator NewEvaluatorWithNewScope()
        {
            var newLocalContext = new LocalContext(localContext);
            return new IR0StmtEvaluator(globalContext, context, newLocalContext, localTaskContext);
        }

        async IAsyncEnumerable<Void> EvalIfTestEnumElemStmtAsync(R.IfTestEnumElemStmt stmt)
        {
            var targetValue = (EnumValue)await IR0LocEvaluator.EvalAsync(globalContext, context, localContext, stmt.Target);
            var enumElemRuntimeItem = globalContext.GetRuntimeItem<EnumElemRuntimeItem>(stmt.EnumElem);

            var bTestPassed = targetValue.IsElem(enumElemRuntimeItem);

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

        async IAsyncEnumerable<Void> EvalIfTestClassStmtAsync(R.IfTestClassStmt stmt)
        {
            // 분석기가 미리 계산해 놓은 TypeValue를 가져온다                
            var targetValue = (ClassValue)await EvalLocAsync(stmt.Target);
            var targetType = targetValue.GetActualType();

            var bTestPassed = IsType(targetType, stmt.TestType);

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
                    var condValue = context.AllocValue<BoolValue>(R.Path.Bool);
                    await EvalExpAsync(forStmt.CondExp, condValue);

                    if (!condValue.GetBool())
                        break;
                }

                await foreach (var value in EvalStmtAsync(forStmt.Body))
                    yield return Void.Instance;

                var flowControl = context.GetFlowControl();

                if (flowControl == EvalFlowControl.Break)
                {
                    context.SetFlowControl(EvalFlowControl.None);
                    break;
                }
                else if (flowControl == EvalFlowControl.Continue)
                {
                    context.SetFlowControl(EvalFlowControl.None);
                }
                else if (flowControl == EvalFlowControl.Return)
                {
                    break;
                }
                else
                {
                    Debug.Assert(context.GetFlowControl() == EvalFlowControl.None);
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
            context.SetFlowControl(EvalFlowControl.Continue);
        }

        void EvalBreakStmt(R.BreakStmt breakStmt)
        {
            context.SetFlowControl(EvalFlowControl.Break);
        }

        async ValueTask EvalReturnStmtAsync(R.ReturnStmt returnStmt)
        {
            switch (returnStmt.Info)
            {
                case R.ReturnInfo.None:
                    break;

                case R.ReturnInfo.Ref refInfo:
                    {
                        var retValue = (RefValue)context.GetRetValue();
                        var target = await EvalLocAsync(refInfo.Loc);
                        retValue.SetTarget(target);
                        break;
                    }

                case R.ReturnInfo.Expression expInfo:
                    {
                        var retValue = context.GetRetValue();
                        await EvalExpAsync(expInfo.Exp, retValue);
                        break;
                    }
            }

            context.SetFlowControl(EvalFlowControl.Return);
        }

        async IAsyncEnumerable<Void> EvalBlockStmtCoreAsync(R.BlockStmt blockStmt)
        {
            foreach (var stmt in blockStmt.Stmts)
            {
                await foreach (var value in EvalStmtAsync(stmt))
                {
                    yield return Void.Instance;

                    // 확실하지 않아서 걸어둔다
                    Debug.Assert(context.GetFlowControl() == EvalFlowControl.None);
                }

                if (context.GetFlowControl() != EvalFlowControl.None)
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
            await IR0ExpEvaluator.EvalAsync(globalContext, context, localContext, expStmt.Exp, EmptyValue.Instance);
        }            

        void EvalTaskStmt(R.TaskStmt taskStmt)
        {
            var runtimeItem = globalContext.GetRuntimeItem<CapturedStmtRuntimeItem>(taskStmt.CapturedStatementDecl);
            runtimeItem.InvokeParallel(globalContext, context, localContext, localTaskContext);
        }

        async IAsyncEnumerable<Void> EvalAwaitStmtAsync(R.AwaitStmt stmt)
        {
            var newLocalTaskContext = new LocalTaskContext();
            var newStmtEvaluator = new IR0StmtEvaluator(globalContext, context, localContext, newLocalTaskContext);                
                
            await foreach (var _ in newStmtEvaluator.EvalStmtAsync(stmt.Body))
                yield return Void.Instance;

            await newLocalTaskContext.WaitAllAsync();
        }

        void EvalAsyncStmt(R.AsyncStmt asyncStmt)
        {
            var runtimeItem = globalContext.GetRuntimeItem<CapturedStmtRuntimeItem>(asyncStmt.CapturedStatementDecl);
            runtimeItem.InvokeAsynchronous(globalContext, context, localContext, localTaskContext);
        }

        async IAsyncEnumerable<Void> EvalForeachStmtCoreAsync(R.ForeachStmt stmt)
        {
            var iteratorLoc = (SeqValue)await EvalLocAsync(stmt.Iterator);
            var elemValue = context.AllocValue(stmt.ElemType);

            localContext.AddLocalVar(new R.Name.Normal(stmt.ElemName), elemValue);
            while (await iteratorLoc.NextAsync(elemValue))
            {
                await foreach (var _ in EvalStmtAsync(stmt.Body))
                {
                    yield return Void.Instance;
                }

                var flowControl = context.GetFlowControl();

                if (flowControl == EvalFlowControl.Break)
                {
                    context.SetFlowControl(EvalFlowControl.None);
                    break;
                }
                else if (flowControl == EvalFlowControl.Continue)
                {
                    context.SetFlowControl(EvalFlowControl.None);
                }
                else if (flowControl == EvalFlowControl.Return)
                {
                    break;
                }
                else
                {
                    Debug.Assert(flowControl == EvalFlowControl.None);
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
            await EvalExpAsync(yieldStmt.Value, context.GetYieldValue());
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
                    EvalTaskStmt(taskStmt);
                    return AsyncEnumerable.Empty<Void>();

                case R.AwaitStmt awaitStmt:
                    return EvalAwaitStmtAsync(awaitStmt);

                case R.AsyncStmt asyncStmt:
                    EvalAsyncStmt(asyncStmt);
                    return AsyncEnumerable.Empty<Void>();

                case R.ForeachStmt foreachStmt:
                    return EvalForeachStmtAsync(foreachStmt);

                case R.YieldStmt yieldStmt:
                    return EvalYieldStmtAsync(yieldStmt);                            

                default:
                    throw new UnreachableCodeException();
            }
        }
 
    }
}