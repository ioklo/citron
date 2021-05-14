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
    abstract class FuncInvoker
    {
        public abstract R.ParamInfo ParamInfo { get; }
        public abstract ValueTask Invoke(Value? thisValue, Value result, ImmutableDictionary<string, Value> args);
    }

    [AutoConstructor]
    partial class IR0FuncInvoker : FuncInvoker
    {
        Evaluator evaluator;
        R.Stmt body;

        public override R.ParamInfo ParamInfo { get; }

        public override async ValueTask Invoke(Value? thisValue, Value result, ImmutableDictionary<string, Value> args)
        {
            await evaluator.ExecInNewFuncFrameAsync(args, EvalFlowControl.None, ImmutableArray<Task>.Empty, thisValue, result, async () => {

                await foreach (var _ in evaluator.EvalStmtAsync(body)) { }

            });
        }
    }
}
