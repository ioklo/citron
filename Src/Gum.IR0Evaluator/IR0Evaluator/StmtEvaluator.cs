using System;
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
    public partial class Evaluator
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
            async ValueTask EvalCommandStmtAsync(R.CommandStmt stmt)
            {
                var tempStr = evaluator.AllocValue<StringValue>(R.Path.String);

                foreach (var command in stmt.Commands)
                {
                    await evaluator.EvalStringExpAsync(command, tempStr);
                    var cmdText = tempStr.GetString();

                    await commandProvider.ExecuteAsync(cmdText);
                }
            }

            async ValueTask EvalGlobalVarDeclStmtAsync(R.GlobalVarDeclStmt stmt)
            {
                foreach (var elem in stmt.Elems)
                {
                    var value = evaluator.AllocValue(elem.Type);                    

                    // InitExp가 있으면 
                    if (elem.InitExp != null)
                        await evaluator.EvalExpAsync(elem.InitExp, value);

                    // 순서 주의, InitExp먼저 실행
                    // TODO: 테스트로 만들기
                    evaluator.context.AddGlobalVar(elem.Name, value);
                }
            }

            async ValueTask EvalGlobalRefVarDeclStmtAsync(R.GlobalRefVarDeclStmt stmt)
            {
                foreach (var elem in stmt.Elems)
                {
                    var value = await evaluator.EvalLocAsync(elem.Loc);
                    evaluator.context.AddGlobalVar(elem.Name, value);
                }
            }

            ValueTask EvalLocalVarDeclStmtAsync(R.LocalVarDeclStmt stmt)
            {
                return evaluator.EvalLocalVarDeclAsync(stmt.VarDecl);
            }

            ValueTask EvalLocalRefVarDeclStmtAsync(R.LocalRefVarDeclStmt stmt)
            {
                return evaluator.EvalLocalRefVarDeclAsync(stmt.RefVarDecl);
            }

            async IAsyncEnumerable<Void> EvalIfStmtAsync(R.IfStmt stmt)
            {
                var condValue = evaluator.AllocValue<BoolValue>(R.Path.Bool);
                await evaluator.EvalExpAsync(stmt.Cond, condValue);

                if (condValue.GetBool())
                {
                    await foreach (var value in EvalStmtAsync(stmt.Body))
                        yield return Void.Instance;
                }
                else
                {
                    if (stmt.ElseBody != null)
                        await foreach (var value in EvalStmtAsync(stmt.ElseBody))
                            yield return Void.Instance;
                }
            }

            async IAsyncEnumerable<Void> EvalIfTestEnumElemStmtAsync(R.IfTestEnumElemStmt stmt)
            {
                var targetValue = (EnumValue)await evaluator.EvalLocAsync(stmt.Target);
                var enumElemRuntimeItem = evaluator.context.GetRuntimeItem<EnumElemRuntimeItem>(stmt.EnumElem);

                var bTestPassed = targetValue.IsElem(enumElemRuntimeItem);

                if (bTestPassed)
                {
                    if (stmt.VarName != null)
                    {
                        async IAsyncEnumerable<Void> InnerFunc()
                        {   
                            evaluator.context.AddLocalVar(stmt.VarName, targetValue.GetElemValue()); // 레퍼런스로 등록

                            await foreach (var _ in EvalStmtAsync(stmt.Body))
                                yield return Void.Instance;
                        }

                        await foreach (var _ in evaluator.context.ExecInNewScopeAsync(InnerFunc))
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
                var targetValue = (ClassValue)await evaluator.EvalLocAsync(stmt.Target);
                var targetType = targetValue.GetType();

                var bTestPassed = evaluator.IsType(targetType, stmt.TestType);

                if (bTestPassed)
                {
                    await foreach (var value in EvalStmtAsync(stmt.Body))
                        yield return Void.Instance;
                }
                else
                {
                    if (stmt.ElseBody != null)
                        await foreach (var value in EvalStmtAsync(stmt.ElseBody))
                            yield return Void.Instance;
                }
            }

            IAsyncEnumerable<Void> EvalForStmtAsync(R.ForStmt forStmt)
            {
                async IAsyncEnumerable<Void> InnerAsync()
                {
                    // continue를 실행시키기 위한 공간은 미리 할당받는다
                    if (forStmt.Initializer != null)
                    {
                        switch (forStmt.Initializer)
                        {   
                            case R.VarDeclForStmtInitializer varDeclInitializer:
                                await evaluator.EvalLocalVarDeclAsync(varDeclInitializer.VarDecl);
                                break;

                            case R.RefVarDeclForStmtInitializer refVarDeclInitializer:
                                await evaluator.EvalLocalRefVarDeclAsync(refVarDeclInitializer.RefVarDecl);
                                break;

                            case R.ExpForStmtInitializer expInitializer:
                                await evaluator.EvalExpAsync(expInitializer.Exp, EmptyValue.Instance);
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }

                    while (true)
                    {
                        if (forStmt.CondExp != null)
                        {
                            var condValue = evaluator.AllocValue<BoolValue>(R.Path.Bool);
                            await evaluator.EvalExpAsync(forStmt.CondExp, condValue);

                            if (!condValue.GetBool())
                                break;
                        }

                        await foreach (var value in EvalStmtAsync(forStmt.Body))
                            yield return Void.Instance;

                        var flowControl = evaluator.context.GetFlowControl();

                        if (flowControl == EvalFlowControl.Break)
                        {
                            evaluator.context.SetFlowControl(EvalFlowControl.None);
                            break;
                        }
                        else if (flowControl == EvalFlowControl.Continue)
                        {
                            evaluator.context.SetFlowControl(EvalFlowControl.None);
                        }
                        else if (flowControl == EvalFlowControl.Return)
                        {
                            break;
                        }
                        else
                        {
                            Debug.Assert(evaluator.context.GetFlowControl() == EvalFlowControl.None);
                        }

                        if (forStmt.ContinueExp != null)
                        {
                            await evaluator.EvalExpAsync(forStmt.ContinueExp, EmptyValue.Instance);
                        }
                    }
                }

                return evaluator.context.ExecInNewScopeAsync(InnerAsync);
            }

            void EvalContinueStmt(R.ContinueStmt continueStmt)
            {
                evaluator.context.SetFlowControl(EvalFlowControl.Continue);
            }

            void EvalBreakStmt(R.BreakStmt breakStmt)
            {
                evaluator.context.SetFlowControl(EvalFlowControl.Break);
            }

            async ValueTask EvalReturnStmtAsync(R.ReturnStmt returnStmt)
            {
                if (returnStmt.Value != null)
                {
                    var retValue = evaluator.context.GetRetValue();
                    await evaluator.EvalExpAsync(returnStmt.Value, retValue);
                }

                evaluator.context.SetFlowControl(EvalFlowControl.Return);
            }

            IAsyncEnumerable<Void> EvalBlockStmtAsync(R.BlockStmt blockStmt)
            {
                async IAsyncEnumerable<Void> InnerAsync()
                {
                    foreach (var stmt in blockStmt.Stmts)
                    {
                        await foreach (var value in EvalStmtAsync(stmt))
                        {
                            yield return Void.Instance;

                            // 확실하지 않아서 걸어둔다
                            Debug.Assert(evaluator.context.GetFlowControl() == EvalFlowControl.None);
                        }

                        if (evaluator.context.GetFlowControl() != EvalFlowControl.None)
                            break;
                    }
                }

                return evaluator.context.ExecInNewScopeAsync(InnerAsync);
            }

            async ValueTask EvalExpStmtAsync(R.ExpStmt expStmt)
            {
                await evaluator.EvalExpAsync(expStmt.Exp, EmptyValue.Instance);
            }            

            void EvalTaskStmt(R.TaskStmt taskStmt)
            {
                var runtimeItem = evaluator.context.GetRuntimeItem<CapturedStmtRuntimeItem>(taskStmt.CapturedStatementDecl);
                runtimeItem.InvokeParallel(evaluator);
            }

            IAsyncEnumerable<Void> EvalAwaitStmtAsync(R.AwaitStmt stmt)
            {
                async IAsyncEnumerable<Void> EvalAsync()
                {
                    await foreach (var value in EvalStmtAsync(stmt.Body))
                        yield return Void.Instance;

                    await Task.WhenAll(evaluator.context.GetTasks().AsEnumerable());
                }

                return evaluator.context.ExecInNewTasks(EvalAsync);
            }

            void EvalAsyncStmt(R.AsyncStmt asyncStmt)
            {
                var runtimeItem = evaluator.context.GetRuntimeItem<CapturedStmtRuntimeItem>(asyncStmt.CapturedStatementDecl);
                runtimeItem.InvokeAsynchronous(evaluator);
            }

            public StmtEvaluator Clone(Evaluator evaluator)
            {
                return new StmtEvaluator(evaluator, commandProvider);
            }

            async IAsyncEnumerable<Void> EvalForeachStmtAsync(R.ForeachStmt stmt)
            {
                async IAsyncEnumerable<Void> InnerScopeAsync()
                {
                    var iteratorLoc = (SeqValue)await evaluator.EvalLocAsync(stmt.Iterator);
                    var elemValue = evaluator.AllocValue(stmt.ElemType);

                    evaluator.context.AddLocalVar(stmt.ElemName, elemValue);
                    while (await iteratorLoc.NextAsync(elemValue))
                    {
                        await foreach (var value in EvalStmtAsync(stmt.Body))
                        {
                            yield return Void.Instance;
                        }

                        var flowControl = evaluator.context.GetFlowControl();

                        if (flowControl == EvalFlowControl.Break)
                        {
                            evaluator.context.SetFlowControl(EvalFlowControl.None);
                            break;
                        }
                        else if (flowControl == EvalFlowControl.Continue)
                        {
                            evaluator.context.SetFlowControl(EvalFlowControl.None);
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

                await foreach (var yieldValue in evaluator.context.ExecInNewScopeAsync(InnerScopeAsync))
                {
                    yield return yieldValue;
                }
            }

            async IAsyncEnumerable<Void> EvalYieldStmtAsync(R.YieldStmt yieldStmt)
            {
                await evaluator.EvalExpAsync(yieldStmt.Value, evaluator.context.GetYieldValue());
                yield return Void.Instance;
            }

            public async IAsyncEnumerable<Void> EvalStmtAsync(R.Stmt stmt)
            {
                switch (stmt)
                {
                    case R.CommandStmt cmdStmt:
                        await EvalCommandStmtAsync(cmdStmt);
                        break;

                    case R.GlobalVarDeclStmt gVarDecl:
                        await EvalGlobalVarDeclStmtAsync(gVarDecl);
                        break;

                    case R.GlobalRefVarDeclStmt gRefVarDecl:
                        await EvalGlobalRefVarDeclStmtAsync(gRefVarDecl);
                        break;

                    case R.LocalVarDeclStmt localVarDeclStmt:
                        await EvalLocalVarDeclStmtAsync(localVarDeclStmt);
                        break;

                    case R.LocalRefVarDeclStmt lRefVarDecl:
                        await EvalLocalRefVarDeclStmtAsync(lRefVarDecl);
                        break;

                    case R.IfStmt ifStmt:
                        await foreach (var _ in EvalIfStmtAsync(ifStmt))
                            yield return Void.Instance;
                        break;

                    case R.IfTestClassStmt ifTestClassStmt:
                        await foreach (var _ in EvalIfTestClassStmtAsync(ifTestClassStmt))
                            yield return Void.Instance;
                        break;

                    case R.IfTestEnumElemStmt ifTestEnumStmt:
                        await foreach (var _ in EvalIfTestEnumElemStmtAsync(ifTestEnumStmt))
                            yield return Void.Instance;
                        break;

                    case R.ForStmt forStmt:
                        await foreach (var _ in EvalForStmtAsync(forStmt))
                            yield return Void.Instance;
                        break;

                    case R.ContinueStmt continueStmt:
                        EvalContinueStmt(continueStmt);
                        break;

                    case R.BreakStmt breakStmt:
                        EvalBreakStmt(breakStmt);
                        break;

                    case R.ReturnStmt returnStmt:
                        await EvalReturnStmtAsync(returnStmt);
                        break;

                    case R.BlockStmt blockStmt:
                        await foreach (var _ in EvalBlockStmtAsync(blockStmt))
                            yield return Void.Instance;
                        break;

                    case R.BlankStmt blankStmt: break;

                    case R.ExpStmt expStmt:
                        await EvalExpStmtAsync(expStmt);
                        break;

                    case R.TaskStmt taskStmt:
                        EvalTaskStmt(taskStmt);
                        break;

                    case R.AwaitStmt awaitStmt:
                        await foreach (var _ in EvalAwaitStmtAsync(awaitStmt))
                            yield return Void.Instance;
                        break;

                    case R.AsyncStmt asyncStmt:
                        EvalAsyncStmt(asyncStmt);
                        break;

                    case R.ForeachStmt foreachStmt:
                        await foreach (var _ in EvalForeachStmtAsync(foreachStmt))
                            yield return Void.Instance;
                        break;

                    case R.YieldStmt yieldStmt:
                        await foreach (var _ in EvalYieldStmtAsync(yieldStmt))
                            yield return Void.Instance;
                        break;

                    default:
                        throw new UnreachableCodeException();
                };
            }
        }
    }
}