using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class CapturedStmtRuntimeItem : RuntimeItem
    {
        public abstract void InvokeParallel(IR0GlobalContext globalContext, IR0EvalContext context, LocalContext localContext, LocalTaskContext localTaskContext);
        public abstract void InvokeAsynchronous(IR0GlobalContext globalContext, IR0EvalContext context, LocalContext localContext, LocalTaskContext localTaskContext);
    }    
        [AutoConstructor]
        partial class IR0CapturedStmtRuntimeItem : CapturedStmtRuntimeItem
        {
            public override R.Name Name => decl.Name;
            public override R.ParamHash ParamHash => R.ParamHash.None;
            R.CapturedStatementDecl decl;

            (Value? ThisValue, ImmutableDictionary<string, Value> LocalVars) Allocate(IR0GlobalContext globalContext)
            {
                Value? thisValue = null;

                if (decl.CapturedStatement.ThisType != null)
                    thisValue = context.AllocValue(decl.CapturedStatement.ThisType);

                var localVarsBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
                foreach (var (type, name) in decl.CapturedStatement.OuterLocalVars)
                {
                    var value = context.AllocValue(type);
                    localVarsBuilder.Add(name, value);
                }

                return (thisValue, localVarsBuilder.ToImmutable());
            }

            public override void InvokeParallel(IR0GlobalContext globalContext, IR0EvalContext context, LocalContext localContext, LocalTaskContext localTaskContext)
            {
                var (capturedThis, capturedLocals) = Allocate(globalContext);
                CaptureLocals(context, localContext, capturedThis, capturedLocals, decl.CapturedStatement);
                
                var task = Task.Run(async () =>
                {
                    var newContext = new IR0EvalContext(capturedLocals, EvalFlowControl.None, capturedThis, VoidValue.Instance);
                    var newLocalContext = new LocalContext(ImmutableDictionary<R.Name, Value>.Empty);
                    var newLocalTaskContext = new LocalTaskContext();
                    await IR0StmtEvaluator.EvalAsync(globalContext, newContext, newLocalContext, newLocalTaskContext, decl.CapturedStatement.Body);
                });

                localTaskContext.AddTask(task);
            }

            public override void InvokeAsynchronous(IR0GlobalContext globalContext, IR0EvalContext context, LocalContext localContext, LocalTaskContext localTaskContext)
            {
                var (capturedThis, capturedLocals) = Allocate(globalContext);
                CaptureLocals(context, localContext, capturedThis, capturedLocals, decl.CapturedStatement);

                async Task WrappedAsyncFunc()
                {
                    var newContext = new IR0EvalContext(capturedLocals, EvalFlowControl.None, capturedThis, VoidValue.Instance);
                    var newLocalContext = new LocalContext(ImmutableDictionary<R.Name, Value>.Empty);
                    var newLocalTaskContext = new LocalTaskContext();

                    await IR0StmtEvaluator.EvalAsync(globalContext, newContext, newLocalContext, newLocalTaskContext, decl.CapturedStatement.Body);
                };

                // 현재 컨텍스트에서 실행
                var task = WrappedAsyncFunc();
                localTaskContext.AddTask(task);
            }
        }
}
