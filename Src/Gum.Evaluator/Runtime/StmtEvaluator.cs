using Gum.Runtime;
using Gum.StaticAnalysis;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Gum.Runtime.Evaluator;
using Gum.CompileTime;
using Gum;

namespace Gum.Runtime
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
        internal async ValueTask EvaluateCommandStmtAsync(CommandStmt stmt, EvalContext context)
        {
            var tempStr = context.RuntimeModule.MakeNullObject();

            foreach (var command in stmt.Commands)
            {
                await evaluator.EvaluateStringExpAsync(command, tempStr, context);
                var cmdText = context.RuntimeModule.GetString(tempStr);

                await commandProvider.ExecuteAsync(cmdText);
            }
        }

        internal ValueTask EvaluateVarDeclStmtAsync(VarDeclStmt stmt, EvalContext context)
        {
            return evaluator.EvaluateVarDeclAsync(stmt.VarDecl, context);
        }

        internal async IAsyncEnumerable<Value> EvaluateIfStmtAsync(IfStmt stmt, EvalContext context)
        {
            bool bTestPassed;
            if (stmt.TestType == null)
            {
                var condValue = context.RuntimeModule.MakeBool(false);
                await evaluator.EvalExpAsync(stmt.Cond, condValue, context);

                bTestPassed = context.RuntimeModule.GetBool(condValue);
            }
            else
            {
                // 분석기가 미리 계산해 놓은 TypeValue를 가져온다
                var ifStmtInfo = context.GetNodeInfo<IfStmtInfo>(stmt);

                if (ifStmtInfo is IfStmtInfo.TestEnum testEnumInfo)
                {
                    var tempEnumValue = (EnumValue)evaluator.GetDefaultValue(testEnumInfo.TestTargetTypeValue, context);
                    await evaluator.EvalExpAsync(stmt.Cond, tempEnumValue, context);

                    bTestPassed = (tempEnumValue.ElemName == testEnumInfo.ElemName);
                }
                else if (ifStmtInfo is IfStmtInfo.TestClass testClassInfo)
                {
                    var tempValue = (ObjectValue)evaluator.GetDefaultValue(testClassInfo.TestTargetTypeValue, context);
                    await evaluator.EvalExpAsync(stmt.Cond, tempValue, context);

                    var condValueTypeValue = tempValue.GetTypeInst().GetTypeValue();

                    Debug.Assert(condValueTypeValue is TypeValue.Normal);
                    bTestPassed = evaluator.IsType((TypeValue.Normal)condValueTypeValue, testClassInfo.TestTypeValue, context);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            if (bTestPassed)
            {
                await foreach (var value in EvaluateStmtAsync(stmt.Body, context))
                    yield return value;
            }
            else
            {
                if (stmt.ElseBody != null)
                    await foreach (var value in EvaluateStmtAsync(stmt.ElseBody, context))
                        yield return value;
            }
        }

        internal async IAsyncEnumerable<Value> EvaluateForStmtAsync(ForStmt forStmt, EvalContext context)
        {
            var forStmtInfo = context.GetNodeInfo<ForStmtInfo>(forStmt);
            var contValue = forStmtInfo.ContTypeValue != null ? evaluator.GetDefaultValue(forStmtInfo.ContTypeValue, context) : null;

            if (forStmt.Initializer != null)
            {
                switch (forStmt.Initializer)
                {
                    case ExpForStmtInitializer expInitializer:
                        var expInitInfo = context.GetNodeInfo<ExpForStmtInitializerInfo>(expInitializer);
                        var value = evaluator.GetDefaultValue(expInitInfo.ExpTypeValue, context);
                        await evaluator.EvalExpAsync(expInitializer.Exp, value, context);
                        break;

                    case VarDeclForStmtInitializer varDeclInitializer:
                        await evaluator.EvaluateVarDeclAsync(varDeclInitializer.VarDecl, context);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            while (true)
            {
                if (forStmt.CondExp != null)
                {
                    var condValue = context.RuntimeModule.MakeBool(false);
                    await evaluator.EvalExpAsync(forStmt.CondExp, condValue, context);                    

                    if (!context.RuntimeModule.GetBool(condValue)) 
                        break;
                }

                await foreach (var value in EvaluateStmtAsync(forStmt.Body, context))
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
                    await evaluator.EvalExpAsync(forStmt.ContinueExp, contValue!, context);
                }
            }
        }

        internal void EvaluateContinueStmt(ContinueStmt continueStmt, EvalContext context)
        {
            context.SetFlowControl(EvalFlowControl.Continue);
        }

        internal void EvaluateBreakStmt(BreakStmt breakStmt, EvalContext context)
        {
            context.SetFlowControl(EvalFlowControl.Break);
        }

        internal async ValueTask EvaluateReturnStmtAsync(ReturnStmt returnStmt, EvalContext context)
        {
            if (returnStmt.Value != null)
            {
                var retValue = context.GetRetValue();
                await evaluator.EvalExpAsync(returnStmt.Value, retValue, context);
            }

            context.SetFlowControl(EvalFlowControl.Return);
        }

        internal async IAsyncEnumerable<Value> EvaluateBlockStmtAsync(BlockStmt blockStmt, EvalContext context)
        {
            foreach (var stmt in blockStmt.Stmts)
            {
                await foreach (var value in EvaluateStmtAsync(stmt, context))
                {
                    yield return value;

                    // 확실하지 않아서 걸어둔다
                    Debug.Assert(context.GetFlowControl() == EvalFlowControl.None);
                }

                if (context.GetFlowControl() != EvalFlowControl.None)
                    break;
            }            
        }

        internal async ValueTask EvaluateExpStmtAsync(ExpStmt expStmt, EvalContext context)
        {
            var expStmtInfo = context.GetNodeInfo<ExpStmtInfo>(expStmt);
            var temp = evaluator.GetDefaultValue(expStmtInfo.ExpTypeValue, context);

            await evaluator.EvalExpAsync(expStmt.Exp, temp, context);
        }

        internal void EvaluateTaskStmt(TaskStmt taskStmt, EvalContext context)
        {
            var info = context.GetNodeInfo<TaskStmtInfo>(taskStmt);

            // 1. funcInst로 캡쳐
            var captures = evaluator.MakeCaptures(info.CaptureInfo.Captures, context);
            
            var funcInst = new ScriptFuncInst(
                null,
                false,
                info.CaptureInfo.bCaptureThis ? context.GetThisValue() : null,
                captures,
                info.LocalVarCount,
                taskStmt.Body);

            var newContext = new EvalContext(context, new Value?[0], EvalFlowControl.None, ImmutableArray<Task>.Empty, null, VoidValue.Instance);

            // 2. 그 funcInst를 바로 실행하기
            var task = Task.Run(async () =>
            {
                await evaluator.EvaluateFuncInstAsync(null, funcInst, ImmutableArray<Value>.Empty, VoidValue.Instance, newContext);
            });

            context.AddTask(task);
        }

        IAsyncEnumerable<Value> EvaluateAwaitStmtAsync(AwaitStmt stmt, EvalContext context)
        {
            async IAsyncEnumerable<Value> EvaluateAsync()
            {
                await foreach (var value in EvaluateStmtAsync(stmt.Body, context))
                    yield return value;

                await Task.WhenAll(context.GetTasks());
            }

            return context.ExecInNewTasks(EvaluateAsync);
        }

        internal void EvaluateAsyncStmt(AsyncStmt asyncStmt, EvalContext context)
        {
            var info = context.GetNodeInfo<AsyncStmtInfo>(asyncStmt);

            var captures = evaluator.MakeCaptures(info.CaptureInfo.Captures, context);

            var funcInst = new ScriptFuncInst(
                null,
                false,
                info.CaptureInfo.bCaptureThis ? context.GetThisValue() : null,
                captures,
                info.LocalVarCount,
                asyncStmt.Body);

            var newContext = new EvalContext(context, new Value?[0], EvalFlowControl.None, ImmutableArray<Task>.Empty, null, VoidValue.Instance);

            Func<Task> asyncFunc = async () =>
            {
                await evaluator.EvaluateFuncInstAsync(null, funcInst, ImmutableArray<Value>.Empty, VoidValue.Instance, newContext);
            };

            var task = asyncFunc();
            context.AddTask(task);
        }

        internal async IAsyncEnumerable<Value> EvaluateForeachStmtAsync(ForeachStmt foreachStmt, EvalContext context)
        {
            var info = context.GetNodeInfo<ForeachStmtInfo>(foreachStmt);

            var objValue = evaluator.GetDefaultValue(info.ObjTypeValue, context);
            var enumeratorValue = evaluator.GetDefaultValue(info.EnumeratorTypeValue, context);
            var moveNextResult = context.RuntimeModule.MakeBool(false);

            await evaluator.EvalExpAsync(foreachStmt.Obj, objValue, context);
            var getEnumeratorInst = context.DomainService.GetFuncInst(info.GetEnumeratorValue);

            await evaluator.EvaluateFuncInstAsync(objValue, getEnumeratorInst, ImmutableArray<Value>.Empty, enumeratorValue, context);
            var moveNextInst = context.DomainService.GetFuncInst(info.MoveNextValue);
            var getCurrentInst = context.DomainService.GetFuncInst(info.GetCurrentValue);

            var elemTypeInst = context.DomainService.GetTypeInst(info.ElemTypeValue);
            context.InitLocalVar(info.ElemLocalIndex, elemTypeInst.MakeDefaultValue());

            while (true)
            {
                await evaluator.EvaluateFuncInstAsync(enumeratorValue, moveNextInst, ImmutableArray<Value>.Empty, moveNextResult, context);
                if (!context.RuntimeModule.GetBool(moveNextResult)) break;

                // GetCurrent
                await evaluator.EvaluateFuncInstAsync(enumeratorValue, getCurrentInst, ImmutableArray<Value>.Empty, context.GetLocalVar(info.ElemLocalIndex), context);

                await foreach (var value in EvaluateStmtAsync(foreachStmt.Body, context))
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

        async IAsyncEnumerable<Value> EvaluateYieldStmtAsync(YieldStmt yieldStmt, EvalContext context)
        {
            await evaluator.EvalExpAsync(yieldStmt.Value, context.GetRetValue(), context);
            yield return context.GetRetValue();
        }
        
        internal async IAsyncEnumerable<Value> EvaluateStmtAsync(Stmt stmt, EvalContext context)
        {
            switch (stmt)
            {
                case CommandStmt cmdStmt: 
                    await EvaluateCommandStmtAsync(cmdStmt, context); 
                    break;

                case VarDeclStmt varDeclStmt: 
                    await EvaluateVarDeclStmtAsync(varDeclStmt, context); 
                    break;

                case IfStmt ifStmt:
                    await foreach (var value in EvaluateIfStmtAsync(ifStmt, context))
                        yield return value;
                    break;

                case ForStmt forStmt:
                    await foreach (var value in EvaluateForStmtAsync(forStmt, context))
                        yield return value;
                    break;

                case ContinueStmt continueStmt: 
                    EvaluateContinueStmt(continueStmt, context); 
                    break;

                case BreakStmt breakStmt: 
                    EvaluateBreakStmt(breakStmt, context); 
                    break;

                case ReturnStmt returnStmt: 
                    await EvaluateReturnStmtAsync(returnStmt, context); 
                    break;

                case BlockStmt blockStmt:
                    await foreach (var result in EvaluateBlockStmtAsync(blockStmt, context))
                        yield return result;
                    break;

                case BlankStmt blankStmt: break;

                case ExpStmt expStmt: 
                    await EvaluateExpStmtAsync(expStmt, context); 
                    break;

                case TaskStmt taskStmt: 
                    EvaluateTaskStmt(taskStmt, context); 
                    break;

                case AwaitStmt awaitStmt:
                    await foreach (var value in EvaluateAwaitStmtAsync(awaitStmt, context))
                        yield return value;
                    break;

                case AsyncStmt asyncStmt: 
                    EvaluateAsyncStmt(asyncStmt, context); 
                    break;

                case ForeachStmt foreachStmt:
                    await foreach (var value in EvaluateForeachStmtAsync(foreachStmt, context))
                        yield return value;
                    break;

                case YieldStmt yieldStmt:
                    await foreach (var result in EvaluateYieldStmtAsync(yieldStmt, context))
                        yield return result;
                    break;

                default: 
                    throw new NotImplementedException();
            };
        }
    }
}