using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0Evaluator
{
    abstract class FuncInvoker
    {
        public abstract void Invoke();
    }

    class IR0FuncInvoker : FuncInvoker
    {
        Evaluator evaluator;
        EvalContext context;

        public override async ValueTask Invoke(Value? thisValue, Value result, ImmutableDictionary<string, Value> args)
        {
            await context.ExecInNewFuncFrameAsync(args, EvalFlowControl.None, ImmutableArray<Task>.Empty, thisValue, result, async () =>
            {
                await foreach (var _ in evaluator.EvalStmtAsync(funcDecl.Body)) { }
            });
        }

    }
}
