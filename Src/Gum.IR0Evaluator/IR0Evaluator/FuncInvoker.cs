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
    abstract class FuncRuntimeItem : RuntimeItem
    {
        public abstract R.ParamInfo ParamInfo { get; }
        public abstract ValueTask InvokeAsync(Evaluator evaluator, Value? thisValue, ImmutableDictionary<string, Value> args, Value result);
    }

    [AutoConstructor]
    partial class IR0FuncRuntimeItem : FuncRuntimeItem
    {
        public override R.Name Name => funcDecl.Name;
        public override R.ParamHash ParamHash => Misc.MakeParamHash(funcDecl.TypeParams.Length, funcDecl.ParamInfo);
        public override R.ParamInfo ParamInfo => funcDecl.ParamInfo;
        R.NormalFuncDecl funcDecl;

        public override async ValueTask InvokeAsync(Evaluator evaluator, Value? thisValue, ImmutableDictionary<string, Value> args, Value result)
        {
            await evaluator.ExecInNewFuncFrameAsync(args, EvalFlowControl.None, ImmutableArray<Task>.Empty, thisValue, result, async () => {

                await foreach (var _ in evaluator.EvalStmtAsync(funcDecl.Body)) { }

            });
        }
    }
}
