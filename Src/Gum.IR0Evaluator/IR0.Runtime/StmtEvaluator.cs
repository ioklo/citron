using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Gum.IR0.Runtime.Evaluator;
using Gum;
using Gum.IR0;

namespace Gum.IR0.Runtime
{
    class StmtEvaluator
    {
        private Evaluator evaluator;
        private ICommandProvider commandProvider;

        public StmtEvaluator(Evaluator evaluator, ICommandProvider commandProvider)
        {
            this.evaluator = evaluator;
            this.commandProvider = commandProvider;
        }

        // TODO: CommandProvider가 Parser도 제공해야 할 것 같다
        internal async ValueTask EvalCommandStmtAsync(CommandStmt stmt, EvalContext context)
        {
            var tempStr = evaluator.AllocValue<StringValue>(Type.String, context);

            foreach (var command in stmt.Commands)
            {
                await evaluator.EvalStringExpAsync(command, tempStr, context);
                var cmdText = tempStr.GetString();

                await commandProvider.ExecuteAsync(cmdText);
            }
        }

        internal async ValueTask EvalPrivateGlobalVarDeclStmtAsync(PrivateGlobalVarDeclStmt stmt, EvalContext context)
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

        internal ValueTask EvalLocalVarDeclStmtAsync(LocalVarDeclStmt stmt, EvalContext context)
        {
            return evaluator.EvalLocalVarDeclAsync(stmt.VarDecl, context);
        }

        internal async IAsyncEnumerable<Value> EvalIfStmtAsync(IfStmt stmt, EvalContext context)
        {
            var condValue = evaluator.AllocValue<BoolValue>(Type.Bool, context);
            await evaluator.EvalExpAsync(stmt.Cond, condValue, context);

            if (condValue.GetBool())
            {
                await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    yield return value;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var value in EvalStmtAsync(stmt.ElseBody, context))
                        yield return value;
            }
        }

        internal async IAsyncEnumerable<Value> EvalIfTestEnumStmtAsync(IfTestEnumStmt stmt, EvalContext context)
        {
            var targetValue = (EnumValue)await evaluator.EvalLocAsync(stmt.Target, context);
            var bTestPassed = (targetValue.GetElemName() == stmt.ElemName);
                
            if (bTestPassed)
            {
                await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    yield return value;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var value in EvalStmtAsync(stmt.ElseBody, context))
                        yield return value;
            }
        }

        internal async IAsyncEnumerable<Value> EvalIfTestClassStmtAsync(IfTestClassStmt stmt, EvalContext context)
        {
            // 분석기가 미리 계산해 놓은 TypeValue를 가져온다                
            var targetValue = (ClassValue)await evaluator.EvalLocAsync(stmt.Target, context);                        
            var targetType = targetValue.GetType();

            var bTestPassed = evaluator.IsType(targetType, stmt.TestType, context);

            if (bTestPassed)
            {
                await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    yield return value;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var value in EvalStmtAsync(stmt.ElseBody, context))
                        yield return value;
            }
        }

        internal IAsyncEnumerable<Value> EvalForStmtAsync(ForStmt forStmt, EvalContext context)
        {
            async IAsyncEnumerable<Value> InnerAsync()
            {
                // continue를 실행시키기 위한 공간은 미리 할당받는다
                var contValue = forStmt.ContinueExp != null ? evaluator.AllocValue(forStmt.ContinueExp.Type, context) : null;

                if (forStmt.Initializer != null)
                {
                    switch (forStmt.Initializer)
                    {
                        case VarDeclForStmtInitializer varDeclInitializer:
                            await evaluator.EvalLocalVarDeclAsync(varDeclInitializer.VarDecl, context);
                            break;

                        case ExpForStmtInitializer expInitializer:
                            await evaluator.EvalLocAsync(expInitializer.Exp, context);
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
                        yield return value;

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
                        await evaluator.EvalExpAsync(forStmt.ContinueExp.Exp, contValue!, context);
                    }
                }
            }

            return context.ExecInNewScopeAsync(InnerAsync);
        }

        internal void EvalContinueStmt(ContinueStmt continueStmt, EvalContext context)
        {
            context.SetFlowControl(EvalFlowControl.Continue);
        }

        internal void EvalBreakStmt(BreakStmt breakStmt, EvalContext context)
        {
            context.SetFlowControl(EvalFlowControl.Break);
        }

        internal async ValueTask EvalReturnStmtAsync(ReturnStmt returnStmt, EvalContext context)
        {
            if (returnStmt.Value != null)
            {
                var retValue = context.GetRetValue();
                await evaluator.EvalExpAsync(returnStmt.Value, retValue, context);
            }

            context.SetFlowControl(EvalFlowControl.Return);
        }

        internal IAsyncEnumerable<Value> EvalBlockStmtAsync(BlockStmt blockStmt, EvalContext context)
        {
            async IAsyncEnumerable<Value> InnerAsync()
            {
                foreach (var stmt in blockStmt.Stmts)
                {
                    await foreach (var value in EvalStmtAsync(stmt, context))
                    {
                        yield return value;

                        // 확실하지 않아서 걸어둔다
                        Debug.Assert(context.GetFlowControl() == EvalFlowControl.None);
                    }

                    if (context.GetFlowControl() != EvalFlowControl.None)
                        break;
                }
            }

            return context.ExecInNewScopeAsync(InnerAsync);
        }

        internal async ValueTask EvalExpStmtAsync(ExpStmt expStmt, EvalContext context)
        {            
            await evaluator.EvalLocAsync(expStmt.Exp, context);
        }

        internal void EvalTaskStmt(TaskStmt taskStmt, EvalContext context)
        {
            var lambdaValue = evaluator.AllocValue<LambdaValue>(taskStmt.LambdaType, context);
            var lambdaDecl = context.GetDecl<LambdaDecl>(lambdaValue.LambdaDeclId);            

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

        IAsyncEnumerable<Value> EvalAwaitStmtAsync(AwaitStmt stmt, EvalContext context)
        {
            async IAsyncEnumerable<Value> EvalAsync()
            {
                await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    yield return value;

                await Task.WhenAll(context.GetTasks().AsEnumerable());
            }

            return context.ExecInNewTasks(EvalAsync);
        }

        internal void EvalAsyncStmt(AsyncStmt asyncStmt, EvalContext context)
        {
            var lambdaValue = evaluator.AllocValue<LambdaValue>(asyncStmt.LambdaType, context);
            var lambdaDecl = context.GetDecl<LambdaDecl>(lambdaValue.LambdaDeclId);

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

        internal async IAsyncEnumerable<Value> EvalForeachStmtAsync(ForeachStmt stmt, EvalContext context)
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            async IAsyncEnumerable<Value> MakeAsyncEnumerable(IEnumerable<Value> enumerable)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                foreach (var elem in enumerable)
                    yield return elem;
            }

            async IAsyncEnumerable<Value> InnerScopeAsync()
            {
                var iterator = await evaluator.EvalLocAsync(stmt.Iterator, context);

                // TODO: iterator는 seq<T> constraint를 따를 수 있고, Enumerable<T> constraint를 따를 수 있다
                // TODO: 현재는 그냥 list<T>이면 enumerable<T>를 에뮬레이션 한다
                IAsyncEnumerable<Value> enumerable;
                if (iterator is ListValue listValue)
                    enumerable = MakeAsyncEnumerable(listValue.GetList());
                else if (iterator is AsyncEnumerableValue enumerableValue)
                    enumerable = enumerableValue.GetAsyncEnumerable();
                else
                    throw new InvalidOperationException();

                var enumerator = enumerable.GetAsyncEnumerator();
                var elemValue = evaluator.AllocValue(stmt.ElemType, context);

                context.AddLocalVar(stmt.ElemName, elemValue);

                while (await enumerator.MoveNextAsync())
                {
                    elemValue.SetValue(enumerator.Current);

                    await foreach (var value in EvalStmtAsync(stmt.Body, context))
                    {
                        yield return value;
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

        async IAsyncEnumerable<Value> EvalYieldStmtAsync(YieldStmt yieldStmt, EvalContext context)
        {
            await evaluator.EvalExpAsync(yieldStmt.Value, context.GetRetValue(), context);
            yield return context.GetRetValue();
        }
        
        internal async IAsyncEnumerable<Value> EvalStmtAsync(Stmt stmt, EvalContext context)
        {
            switch (stmt)
            {
                case CommandStmt cmdStmt: 
                    await EvalCommandStmtAsync(cmdStmt, context); 
                    break;

                case PrivateGlobalVarDeclStmt pgvdStmt:
                    await EvalPrivateGlobalVarDeclStmtAsync(pgvdStmt, context);
                    break;

                case LocalVarDeclStmt localVarDeclStmt:
                    await EvalLocalVarDeclStmtAsync(localVarDeclStmt, context);
                    break;

                case IfStmt ifStmt:
                    await foreach (var value in EvalIfStmtAsync(ifStmt, context))
                        yield return value;
                    break;

                case IfTestClassStmt ifTestClassStmt:
                    await foreach (var value in EvalIfTestClassStmtAsync(ifTestClassStmt, context))
                        yield return value;
                    break;

                case IfTestEnumStmt ifTestEnumStmt:
                    await foreach (var value in EvalIfTestEnumStmtAsync(ifTestEnumStmt, context))
                        yield return value;
                    break;

                case ForStmt forStmt:
                    await foreach (var value in EvalForStmtAsync(forStmt, context))
                        yield return value;
                    break;

                case ContinueStmt continueStmt: 
                    EvalContinueStmt(continueStmt, context); 
                    break;

                case BreakStmt breakStmt: 
                    EvalBreakStmt(breakStmt, context); 
                    break;

                case ReturnStmt returnStmt: 
                    await EvalReturnStmtAsync(returnStmt, context); 
                    break;

                case BlockStmt blockStmt:
                    await foreach (var result in EvalBlockStmtAsync(blockStmt, context))
                        yield return result;
                    break;

                case BlankStmt blankStmt: break;

                case ExpStmt expStmt: 
                    await EvalExpStmtAsync(expStmt, context); 
                    break;

                case TaskStmt taskStmt: 
                    EvalTaskStmt(taskStmt, context); 
                    break;

                case AwaitStmt awaitStmt:
                    await foreach (var value in EvalAwaitStmtAsync(awaitStmt, context))
                        yield return value;
                    break;

                case AsyncStmt asyncStmt: 
                    EvalAsyncStmt(asyncStmt, context); 
                    break;

                case ForeachStmt foreachStmt:
                    await foreach (var value in EvalForeachStmtAsync(foreachStmt, context))
                        yield return value;
                    break;

                case YieldStmt yieldStmt:
                    await foreach (var result in EvalYieldStmtAsync(yieldStmt, context))
                        yield return result;
                    break;

                default: 
                    throw new NotImplementedException();
            };
        }
    }
}