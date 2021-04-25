﻿using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Gum.IR0Evaluator.Evaluator;
using Gum;
using Gum.Infra;

using Void = Gum.Infra.Void;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    class StmtEvaluator
    {
        Evaluator evaluator;
        ICommandProvider commandProvider;

        public StmtEvaluator(Evaluator evaluator, ICommandProvider commandProvider)
        {
            this.evaluator = evaluator;
            this.commandProvider = commandProvider;
        }

        // TODO: CommandProvider가 Parser도 제공해야 할 것 같다
        internal async ValueTask EvalCommandStmtAsync(R.CommandStmt stmt, EvalContext context)
        {
            var tempStr = evaluator.AllocValue<StringValue>(R.Type.String, context);

            foreach (var command in stmt.Commands)
            {
                await evaluator.EvalStringExpAsync(command, tempStr, context);
                var cmdText = tempStr.GetString();

                await commandProvider.ExecuteAsync(cmdText);
            }
        }

        internal async ValueTask EvalPrivateGlobalVarDeclStmtAsync(R.PrivateGlobalVarDeclStmt stmt, EvalContext context)
        {
            foreach (var elem in stmt.Elems)
            {
                var value = evaluator.AllocValue(elem.Type, context);
                context.AddPrivateGlobalVar(elem.Name, value);

                // InitExp가 있으면 
                if (elem.InitExp != null)
                    await evaluator.EvalExpAsync(elem.InitExp, value, context);
            }
        }

        internal ValueTask EvalLocalVarDeclStmtAsync(R.LocalVarDeclStmt stmt, EvalContext context)
        {
            return evaluator.EvalLocalVarDeclAsync(stmt.VarDecl, context);
        }

        internal async IAsyncEnumerable<Void> EvalIfStmtAsync(R.IfStmt stmt, EvalContext context)
        {
            var condValue = evaluator.AllocValue<BoolValue>(R.Type.Bool, context);
            await evaluator.EvalExpAsync(stmt.Cond, condValue, context);

            if (condValue.GetBool())
            {
                await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    yield return Void.Instance;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var value in EvalStmtAsync(stmt.ElseBody, context))
                        yield return Void.Instance;
            }
        }

        internal async IAsyncEnumerable<Void> EvalIfTestEnumStmtAsync(R.IfTestEnumStmt stmt, EvalContext context)
        {
            var targetValue = (EnumValue)await evaluator.EvalLocAsync(stmt.Target, context);
            var bTestPassed = (targetValue.GetElemName() == stmt.ElemName);
                
            if (bTestPassed)
            {
                await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    yield return Void.Instance;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var value in EvalStmtAsync(stmt.ElseBody, context))
                        yield return Void.Instance;
            }
        }

        internal async IAsyncEnumerable<Void> EvalIfTestClassStmtAsync(R.IfTestClassStmt stmt, EvalContext context)
        {
            // 분석기가 미리 계산해 놓은 TypeValue를 가져온다                
            var targetValue = (ClassValue)await evaluator.EvalLocAsync(stmt.Target, context);                        
            var targetType = targetValue.GetType();

            var bTestPassed = evaluator.IsType(targetType, stmt.TestType, context);

            if (bTestPassed)
            {
                await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    yield return Void.Instance;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var value in EvalStmtAsync(stmt.ElseBody, context))
                        yield return Void.Instance;
            }
        }

        internal IAsyncEnumerable<Void> EvalForStmtAsync(R.ForStmt forStmt, EvalContext context)
        {
            async IAsyncEnumerable<Void> InnerAsync()
            {
                // continue를 실행시키기 위한 공간은 미리 할당받는다
                if (forStmt.Initializer != null)
                {
                    switch (forStmt.Initializer)
                    {
                        case R.VarDeclForStmtInitializer varDeclInitializer:
                            await evaluator.EvalLocalVarDeclAsync(varDeclInitializer.VarDecl, context);
                            break;

                        case R.ExpForStmtInitializer expInitializer:
                            await evaluator.EvalExpAsync(expInitializer.Exp, EmptyValue.Instance, context);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                while (true)
                {
                    if (forStmt.CondExp != null)
                    {
                        var condValue = evaluator.AllocValue<BoolValue>(Type.Bool, context);
                        await evaluator.EvalExpAsync(forStmt.CondExp, condValue, context);

                        if (!condValue.GetBool())
                            break;
                    }

                    await foreach (var value in EvalStmtAsync(forStmt.Body, context))
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
                        await evaluator.EvalExpAsync(forStmt.ContinueExp, EmptyValue.Instance, context);
                    }
                }
            }

            return context.ExecInNewScopeAsync(InnerAsync);
        }

        internal void EvalContinueStmt(R.ContinueStmt continueStmt, EvalContext context)
        {
            context.SetFlowControl(EvalFlowControl.Continue);
        }

        internal void EvalBreakStmt(R.BreakStmt breakStmt, EvalContext context)
        {
            context.SetFlowControl(EvalFlowControl.Break);
        }

        internal async ValueTask EvalReturnStmtAsync(R.ReturnStmt returnStmt, EvalContext context)
        {
            if (returnStmt.Value != null)
            {
                var retValue = context.GetRetValue();
                await evaluator.EvalExpAsync(returnStmt.Value, retValue, context);
            }

            context.SetFlowControl(EvalFlowControl.Return);
        }

        internal IAsyncEnumerable<Void> EvalBlockStmtAsync(R.BlockStmt blockStmt, EvalContext context)
        {
            async IAsyncEnumerable<Void> InnerAsync()
            {
                foreach (var stmt in blockStmt.Stmts)
                {
                    await foreach (var value in EvalStmtAsync(stmt, context))
                    {
                        yield return Void.Instance;

                        // 확실하지 않아서 걸어둔다
                        Debug.Assert(context.GetFlowControl() == EvalFlowControl.None);
                    }

                    if (context.GetFlowControl() != EvalFlowControl.None)
                        break;
                }
            }

            return context.ExecInNewScopeAsync(InnerAsync);
        }

        internal async ValueTask EvalExpStmtAsync(R.ExpStmt expStmt, EvalContext context)
        {            
            await evaluator.EvalExpAsync(expStmt.Exp, EmptyValue.Instance, context);
        }

        internal void EvalTaskStmt(R.TaskStmt taskStmt, EvalContext context)
        {
            var lambdaValue = evaluator.AllocValue<LambdaValue>(taskStmt.LambdaType, context);
            var lambdaDecl = context.GetDecl<R.LambdaDecl>(lambdaValue.LambdaDeclId);            

            evaluator.Capture(
                lambdaValue, 
                lambdaDecl.CapturedThisType != null, 
                ImmutableArray.CreateRange(lambdaDecl.CaptureInfo, captureInfo => captureInfo.Name),
                context);
            
            var newContext = new EvalContext(context, default, EvalFlowControl.None, default, null, VoidValue.Instance);

            // 2. 그 lambda를 바로 실행하기
            var task = Task.Run(async () =>
            {
                await evaluator.EvalLambdaAsync(lambdaValue, default, VoidValue.Instance, newContext);
            });

            context.AddTask(task);
        }

        IAsyncEnumerable<Void> EvalAwaitStmtAsync(R.AwaitStmt stmt, EvalContext context)
        {
            async IAsyncEnumerable<Void> EvalAsync()
            {
                await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    yield return Void.Instance;

                await Task.WhenAll(context.GetTasks().AsEnumerable());
            }

            return context.ExecInNewTasks(EvalAsync);
        }

        internal void EvalAsyncStmt(R.AsyncStmt asyncStmt, EvalContext context)
        {
            var lambdaValue = evaluator.AllocValue<LambdaValue>(asyncStmt.LambdaType, context);
            var lambdaDecl = context.GetDecl<R.LambdaDecl>(lambdaValue.LambdaDeclId);

            evaluator.Capture(
                lambdaValue,
                lambdaDecl.CapturedThisType != null,
                ImmutableArray.CreateRange(lambdaDecl.CaptureInfo, captureInfo => captureInfo.Name),
                context);
            
            var newContext = new EvalContext(context, default, EvalFlowControl.None, default, null, VoidValue.Instance);

            Func<Task> asyncFunc = async () =>
            {
                await evaluator.EvalLambdaAsync(lambdaValue, default, VoidValue.Instance, newContext);
            };

            var task = asyncFunc();
            context.AddTask(task);
        }

        internal async IAsyncEnumerable<Void> EvalForeachStmtAsync(R.ForeachStmt stmt, EvalContext context)
        {
            SeqValue MakeSeqValue(ListValue listValue)
            {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                async IAsyncEnumerator<Void> MakeAsyncEnumerator(IEnumerable<Value> enumerable, EvalContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                {
                    foreach (var value in enumerable)
                    {
                        context.GetYieldValue().SetValue(value);
                        yield return Void.Instance;
                    }
                }

                var newContext = new EvalContext(context, default, EvalFlowControl.None, default, null, VoidValue.Instance);

                return new SeqValue(MakeAsyncEnumerator(listValue.GetList(), newContext), newContext);
            }   

            async IAsyncEnumerable<Void> InnerScopeAsync()
            {
                var iterator = await evaluator.EvalLocAsync(stmt.Iterator, context);

                // TODO: iterator는 seq<T> constraint를 따를 수 있고, Enumerable<T> constraint를 따를 수 있다
                // TODO: 현재는 그냥 list<T>이면 enumerable<T>를 에뮬레이션 한다
                SeqValue seqValue;

                if (iterator is SeqValue)
                    seqValue = (SeqValue)iterator;
                else if (iterator is ListValue listValue)
                    seqValue = MakeSeqValue(listValue);
                else
                    throw new InvalidOperationException();
                
                var elemValue = evaluator.AllocValue(stmt.ElemType, context);

                context.AddLocalVar(stmt.ElemName, elemValue);
                while (await seqValue.NextAsync(elemValue))
                { 
                    await foreach (var value in EvalStmtAsync(stmt.Body, context))
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

            await foreach (var yieldValue in context.ExecInNewScopeAsync(InnerScopeAsync)) 
            {
                yield return yieldValue;
            }
        }

        async IAsyncEnumerable<Void> EvalYieldStmtAsync(R.YieldStmt yieldStmt, EvalContext context)
        {
            await evaluator.EvalExpAsync(yieldStmt.Value, context.GetYieldValue(), context);
            yield return Void.Instance;
        }
        
        internal async IAsyncEnumerable<Void> EvalStmtAsync(R.Stmt stmt, EvalContext context)
        {
            switch (stmt)
            {
                case R.CommandStmt cmdStmt: 
                    await EvalCommandStmtAsync(cmdStmt, context); 
                    break;

                case R.PrivateGlobalVarDeclStmt pgvdStmt:
                    await EvalPrivateGlobalVarDeclStmtAsync(pgvdStmt, context);
                    break;

                case R.LocalVarDeclStmt localVarDeclStmt:
                    await EvalLocalVarDeclStmtAsync(localVarDeclStmt, context);
                    break;

                case R.IfStmt ifStmt:
                    await foreach (var _ in EvalIfStmtAsync(ifStmt, context))
                        yield return Void.Instance;
                    break;

                case R.IfTestClassStmt ifTestClassStmt:
                    await foreach (var _ in EvalIfTestClassStmtAsync(ifTestClassStmt, context))
                        yield return Void.Instance;
                    break;

                case R.IfTestEnumStmt ifTestEnumStmt:
                    await foreach (var _ in EvalIfTestEnumStmtAsync(ifTestEnumStmt, context))
                        yield return Void.Instance;
                    break;

                case R.ForStmt forStmt:
                    await foreach (var _ in EvalForStmtAsync(forStmt, context))
                        yield return Void.Instance;
                    break;

                case R.ContinueStmt continueStmt: 
                    EvalContinueStmt(continueStmt, context); 
                    break;

                case R.BreakStmt breakStmt: 
                    EvalBreakStmt(breakStmt, context); 
                    break;

                case R.ReturnStmt returnStmt: 
                    await EvalReturnStmtAsync(returnStmt, context); 
                    break;

                case R.BlockStmt blockStmt:
                    await foreach (var _ in EvalBlockStmtAsync(blockStmt, context))
                        yield return Void.Instance;
                    break;

                case R.BlankStmt blankStmt: break;

                case R.ExpStmt expStmt: 
                    await EvalExpStmtAsync(expStmt, context); 
                    break;

                case R.TaskStmt taskStmt: 
                    EvalTaskStmt(taskStmt, context); 
                    break;

                case R.AwaitStmt awaitStmt:
                    await foreach (var _ in EvalAwaitStmtAsync(awaitStmt, context))
                        yield return Void.Instance;
                    break;

                case R.AsyncStmt asyncStmt: 
                    EvalAsyncStmt(asyncStmt, context); 
                    break;

                case R.ForeachStmt foreachStmt:
                    await foreach (var _ in EvalForeachStmtAsync(foreachStmt, context))
                        yield return Void.Instance;
                    break;

                case R.YieldStmt yieldStmt:
                    await foreach(var _ in EvalYieldStmtAsync(yieldStmt, context))
                        yield return Void.Instance;
                    break;

                default: 
                    throw new NotImplementedException();
            };
        }
    }
}