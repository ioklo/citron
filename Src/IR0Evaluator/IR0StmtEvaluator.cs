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
using Citron.Symbol;
using Citron.IR0;

namespace Citron
{
    struct IR0StmtEvaluator : IIR0StmtVisitor<IAsyncEnumerable<bool>>
    {
        IR0EvalContext context;

        public IR0StmtEvaluator(IR0EvalContext context)
        {
            this.context = context;
        }

        public static IAsyncEnumerable<bool> EvalAsync(Stmt stmt, IR0EvalContext context)
        {
            var evaluator = new IR0StmtEvaluator(context);
            return stmt.Accept<IR0StmtEvaluator, IAsyncEnumerable<bool>>(ref evaluator);
        }

        //public static async IAsyncEnumerator<bool> EvalAsyncEnum(IR0GlobalContext globalContext, IR0BodyContext bodyContext, IR0LocalContext localContext, Stmt stmt)
        //{
        //    // asyncEnum을 만들기 위해서 내부 함수를 씁니다
        //    var evalContext = new IR0EvalContext(globalContext, bodyContext, localContext);
        //    var stmtEvaluator = new IR0StmtEvaluator(evalContext);

        //    // TODO: 이거 그냥 리턴하는거랑 다른가?
        //    await foreach (var b in stmt.Accept<IR0StmtEvaluator, IAsyncEnumerable<bool>>(ref stmtEvaluator))
        //    {
        //        yield return b;
        //    }
        //} 

        // 복사를 떠야 한다
        IAsyncEnumerable<bool> EvalWithNewEnumVarAsync(EnumValue targetValue, string varName, ImmutableArray<Stmt> body)
        {
            throw new NotImplementedException();

            //var refValue = evalContext.AllocValue();
            //refValue.SetTarget(targetValue.GetElemValue());

            //localContext.AddLocalVar(new Name.Normal(varName), refValue); // 레퍼런스로 등록 => 수정

            //await foreach (var _ in EvalBodyAsync(body))
            //    yield return true;
        }       

        async IAsyncEnumerable<bool> EvalForStmtCoreAsync(ForStmt forStmt)
        {
            // continue를 실행시키기 위한 공간은 미리 할당받는다
            if (forStmt.InitStmts.Length != 0)
            {
                await foreach (var _ in EvalBodyAsync(forStmt.InitStmts))
                    yield return true;                
            }

            while (true)
            {
                if (forStmt.CondExp != null)
                {
                    var condValue = context.AllocValue<BoolValue>(TypeIds.Bool);
                    await IR0ExpEvaluator.EvalAsync(forStmt.CondExp, context, condValue);

                    if (!condValue.GetBool())
                        break;
                }

                await foreach (var _ in EvalBodyAsync(forStmt.Body))
                    yield return true;

                var flowControl = context.GetFlowControl();

                if (flowControl == IR0EvalFlowControl.Break)
                {
                    context.SetFlowControl(IR0EvalFlowControl.None);
                    break;
                }
                else if (flowControl == IR0EvalFlowControl.Continue)
                {
                    context.SetFlowControl(IR0EvalFlowControl.None);
                }
                else if (flowControl == IR0EvalFlowControl.Return)
                {
                    break;
                }
                else
                {
                    Debug.Assert(context.GetFlowControl() == IR0EvalFlowControl.None);
                }

                if (forStmt.ContinueExp != null)
                {
                    await IR0ExpEvaluator.EvalAsync(forStmt.ContinueExp, context, EmptyValue.Instance);
                }
            }
        }
        
        async IAsyncEnumerable<bool> EvalBlockStmtCoreAsync(BlockStmt blockStmt)
        {
            foreach (var stmt in blockStmt.Stmts)
            {
                await foreach (var b in EvalAsync(stmt, context))
                {
                    yield return b;

                    // 확실하지 않아서 걸어둔다
                    Debug.Assert(context.GetFlowControl() == IR0EvalFlowControl.None);
                }

                if (context.GetFlowControl() != IR0EvalFlowControl.None)
                    break;
            }
        }        
        
        async IAsyncEnumerable<bool> EvalForeachStmtCoreAsync(ForeachStmt stmt)
        {
            var iteratorLoc = (SeqValue)await IR0LocEvaluator.EvalAsync(stmt.Iterator, context);
            var elemValue = context.AllocValue(stmt.ItemType);

            context.AddLocalVar(new Name.Normal(stmt.ElemName), elemValue);
            while (await iteratorLoc.NextAsync(elemValue))
            {
                await foreach (var _ in EvalBodyAsync(stmt.Body))
                {
                    yield return true;
                }

                var flowControl = context.GetFlowControl();

                if (flowControl == IR0EvalFlowControl.Break)
                {
                    context.SetFlowControl(IR0EvalFlowControl.None);
                    break; 
                }
                else if (flowControl == IR0EvalFlowControl.Continue)
                {
                    context.SetFlowControl(IR0EvalFlowControl.None);
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

        public async IAsyncEnumerable<bool> EvalBodyAsync(ImmutableArray<Stmt> body)
        {
            foreach(var stmt in body)
            {
                await foreach (var _ in EvalAsync(stmt, context))
                    yield return true;

                if (context.GetFlowControl() == IR0EvalFlowControl.Return)
                    break;
            }
        }

        // yield를 모두 건너뛴다
        public async ValueTask EvalStmtSkipYieldAsync(Stmt stmt)
        {
            await foreach (var _ in EvalAsync(stmt, context)) { }
        }

        // yield를 모두 건너뛴다
        public async ValueTask EvalBodySkipYieldAsync(ImmutableArray<Stmt> body)
        {
            foreach (var stmt in body)
            {
                await foreach (var _ in EvalAsync(stmt, context)) { }

                if (context.GetFlowControl() == IR0EvalFlowControl.Return)
                    break;
            }
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitCommand(CommandStmt stmt)
        {
            // TODO: CommandProvider가 Parser도 제공해야 할 것 같다
            var tempStr = context.AllocValue<StringValue>(TypeIds.String);

            foreach (var command in stmt.Commands)
            {
                var evaluator = new IR0ExpEvaluator(context, tempStr);
                await command.Accept<IR0ExpEvaluator, ValueTask>(ref evaluator);

                var cmdText = tempStr.GetString();

                await context.ExecuteCommandAsync(cmdText);
            }

            yield break;
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitLocalVarDecl(LocalVarDeclStmt stmt)
        {
            var value = context.AllocValue(stmt.Type);

            if (stmt.InitExp != null)
                await IR0ExpEvaluator.EvalAsync(stmt.InitExp, context, value);

            // 없으면 언젠가 할 것이다

            // 순서 주의, TODO: 테스트로 만들기
            context.AddLocalVar(new Name.Normal(stmt.Name), value);

            yield break;
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitIf(IfStmt stmt)
        {
            var condValue = context.AllocValue<BoolValue>(TypeIds.Bool);
            await IR0ExpEvaluator.EvalAsync(stmt.Cond, context, condValue);

            if (condValue.GetBool())
            {
                await foreach (var _ in EvalBodyAsync(stmt.Body))
                    yield return true;
            }
            else
            {
                if (stmt.ElseBody.Length != 0)
                    await foreach (var _ in EvalBodyAsync(stmt.ElseBody))
                        yield return true;
            }
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitIfTestClass(IfTestClassStmt stmt)
        {
            // class B {}, class C<X> : B { }, class D<X> : C<X> { } 의 경우
            // void Func<T>() {
            //     B b = new D<int>();  // b의 겉보기 타입 B, 실제 타입 D<int>
            //     if (b is C<T>) ...   // b의 실제타입 D<int>는 C<T>인가?
            // }

            var targetValue = (ClassValue)await IR0LocEvaluator.EvalAsync(stmt.Target, context);

            // evalContext 상에서 둘이 같은지 비교해야 한다
            // E |- targetType is stmt.Class
            var bTestPassed = context.IsDerivedClassOf(targetValue, stmt.Class);

            if (bTestPassed)
            {
                await foreach (var _ in EvalBodyAsync(stmt.Body))
                    yield return true;
            }
            else
            {
                if (stmt.ElseBody.Length != 0)
                    await foreach (var _ in EvalBodyAsync(stmt.ElseBody))
                        yield return true;
            }
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitIfTestEnumElem(IfTestEnumElemStmt stmt)
        {
            var targetValue = await IR0LocEvaluator.EvalAsync(stmt.Target, context) as EnumValue;
            if (targetValue == null)
                throw new RuntimeFatalException();

            var bTestPassed = context.IsEnumElem(targetValue, stmt.EnumElem);

            if (bTestPassed)
            {
                if (stmt.VarName != null)
                {
                    var newContext = context.NewScopeContext();
                    var newStmtEvaluator = new IR0StmtEvaluator(newContext);

                    await foreach (var _ in newStmtEvaluator.EvalWithNewEnumVarAsync(targetValue, stmt.VarName, stmt.Body))
                        yield return true;
                }
                else
                {
                    await foreach (var _ in EvalBodyAsync(stmt.Body))
                        yield return true;
                }
            }
            else
            {
                if (stmt.ElseBody.Length != 0)
                    await foreach (var _ in EvalBodyAsync(stmt.ElseBody))
                        yield return true;
            }
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitFor(ForStmt stmt)
        {
            var newContext = context.NewScopeContext();
            var newEvaluator = new IR0StmtEvaluator(newContext);
            return newEvaluator.EvalForStmtCoreAsync(stmt);
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitContinue(ContinueStmt stmt)
        {
            context.SetFlowControl(IR0EvalFlowControl.Continue);
            return AsyncEnumerable.Empty<bool>();
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitBreak(BreakStmt stmt)
        {
            context.SetFlowControl(IR0EvalFlowControl.Break);
            return AsyncEnumerable.Empty<bool>();
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitReturn(ReturnStmt stmt)
        {
            switch (stmt.Info)
            {
                case ReturnInfo.None:
                    break;

                case ReturnInfo.Expression expInfo:
                    {
                        var retValue = context.GetRetValue();
                        await IR0ExpEvaluator.EvalAsync(expInfo.Exp, context, retValue);
                        break;
                    }
            }

            context.SetFlowControl(IR0EvalFlowControl.Return);
            yield break;
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitBlock(BlockStmt stmt)
        {
            var newContext = context.NewScopeContext();
            var newEvaluator = new IR0StmtEvaluator(newContext);
            return newEvaluator.EvalBlockStmtCoreAsync(stmt);
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitBlank(BlankStmt stmt)
        {
            return AsyncEnumerable.Empty<bool>();
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitExp(ExpStmt stmt)
        {
            // 할당을 해야하지만, Value기반 구현에서는 할 필요 없다
            await IR0ExpEvaluator.EvalAsync(stmt.Exp, context, EmptyValue.Instance);
            yield break;
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitTask(TaskStmt stmt)
        {
            // 공간 할당
            var lambdaValue = context.AllocValue<LambdaValue>(stmt.Lambda.GetSymbolId());

            // 캡쳐
            await IR0ExpEvaluator.EvalCaptureArgs(stmt.Lambda, lambdaValue, stmt.CaptureArgs, context);

            // 새로운 컨텍스트             
            var newContext = context.NewLambdaContext(lambdaValue, VoidValue.Instance, localVars: default);

            // 스레드풀에서 실행
            var task = Task.Run(async () =>
            {
                var newStmtEvaluator = new IR0StmtEvaluator(newContext);
                var body = newContext.GetBodyStmt(stmt.Lambda.GetSymbolId());
                await newStmtEvaluator.EvalBodySkipYieldAsync(body);
            });

            context.AddTask(task);
            yield break;
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitAwait(AwaitStmt stmt)
        {
            var newContext = context.NewTaskLocalContext();
            var newStmtEvaluator = new IR0StmtEvaluator(newContext);

            await foreach (var _ in newStmtEvaluator.EvalBodyAsync(stmt.Body))
                yield return true;

            await newContext.WaitAllAsync();
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitAsync(AsyncStmt stmt)
        {
            // 공간 할당
            var lambdaValue = context.AllocValue<LambdaValue>(stmt.Lambda.GetSymbolId());

            // 캡쳐
            await IR0ExpEvaluator.EvalCaptureArgs(stmt.Lambda, lambdaValue, stmt.CaptureArgs, context);

            // 새로운 컨텍스트 
            var newContext = context.NewLambdaContext(lambdaValue, VoidValue.Instance, localVars: default);

            // 현재 컨텍스트에서 실행
            var taskCompletionSource = new TaskCompletionSource();
            context.AddTask(taskCompletionSource.Task);

            var newStmtEvaluator = new IR0StmtEvaluator(newContext);

            var body = context.GetBodyStmt(stmt.Lambda.GetSymbolId());
            await newStmtEvaluator.EvalBodySkipYieldAsync(body);
            taskCompletionSource.SetResult();

            yield break;
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitForeach(ForeachStmt stmt)
        {
            var newContext = context.NewScopeContext();
            var newStmtEvaluator = new IR0StmtEvaluator(newContext);

            return newStmtEvaluator.EvalForeachStmtCoreAsync(stmt);
        }

        async IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitYield(YieldStmt stmt)
        {
            await IR0ExpEvaluator.EvalAsync(stmt.Value, context, context.GetYieldValue());
            yield return true;
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitCallClassConstructor(CallClassConstructorStmt stmt)
        {
            throw new NotImplementedException();
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitCallStructConstructor(CallStructConstructorStmt stmt)
        {
            throw new NotImplementedException();
        }

        IAsyncEnumerable<bool> IIR0StmtVisitor<IAsyncEnumerable<bool>>.VisitDirective(DirectiveStmt stmt)
        {
            throw new NotImplementedException();
        }
    }
}