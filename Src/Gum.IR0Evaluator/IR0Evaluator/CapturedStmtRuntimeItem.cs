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
        public abstract void InvokeParallel(Evaluator evaluator);
        public abstract void InvokeAsynchronous(Evaluator evaluator);
    }

    public partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0CapturedStmtRuntimeItem : CapturedStmtRuntimeItem
        {
            public override R.Name Name => decl.Name;
            public override R.ParamHash ParamHash => R.ParamHash.None;
            R.CapturedStatementDecl decl;

            (Value? ThisValue, ImmutableDictionary<string, Value> LocalVars) Allocate(Evaluator evaluator)
            {
                Value? thisValue = null;

                if (decl.CapturedStatement.ThisType != null)
                    thisValue = evaluator.AllocValue(decl.CapturedStatement.ThisType);

                var localVarsBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
                foreach (var (type, name) in decl.CapturedStatement.OuterLocalVars)
                {
                    var value = evaluator.AllocValue(type);
                    localVarsBuilder.Add(name, value);
                }

                return (thisValue, localVarsBuilder.ToImmutable());
            }

            public override void InvokeParallel(Evaluator evaluator)
            {
                var (capturedThis, capturedLocals) = Allocate(evaluator);
                evaluator.CaptureLocals(capturedThis, capturedLocals, decl.CapturedStatement);

                // 새 evaluator를 만들고
                var newEvaluator = evaluator.CloneWithNewContext(capturedThis, capturedLocals, default);

                var task = Task.Run(async () =>
                {
                    await foreach (var _ in newEvaluator.EvalStmtAsync(decl.CapturedStatement.Body)) { }
                });

                evaluator.context.AddTask(task);
            }

            public override void InvokeAsynchronous(Evaluator evaluator)
            {
                var (capturedThis, capturedLocalVars) = Allocate(evaluator);
                evaluator.CaptureLocals(capturedThis, capturedLocalVars, decl.CapturedStatement);

                var newEvaluator = evaluator.CloneWithNewContext(capturedThis, capturedLocalVars, default);

                async Task WrappedAsyncFunc()
                {
                    await foreach (var _ in newEvaluator.EvalStmtAsync(decl.CapturedStatement.Body)) { }
                };

                // 현재 컨텍스트에서 실행
                var task = WrappedAsyncFunc();
                evaluator.context.AddTask(task);
            }
        }
    }
    
}
