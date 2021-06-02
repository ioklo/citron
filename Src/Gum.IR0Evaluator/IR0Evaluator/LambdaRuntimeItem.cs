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
    abstract class LambdaRuntimeItem : AllocatableRuntimeItem
    {
        public abstract R.ParamInfo ParamInfo { get; }
        
        public abstract ValueTask InvokeAsync(Evaluator evaluator, Value? capturedThis, ImmutableDictionary<string, Value> capturedVars, ImmutableDictionary<string, Value> localVars, Value result);
        public abstract void Capture(Evaluator evaluator, LambdaValue lambdaValue);
    }

    public partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0LambdaRuntimeItem : LambdaRuntimeItem
        {
            public override R.Name Name => lambdaDecl.Name;
            public override R.ParamHash ParamHash => R.ParamHash.None;
            public override R.ParamInfo ParamInfo => lambdaDecl.ParamInfo;
            R.LambdaDecl lambdaDecl;

            public override Value Alloc(Evaluator evaluator, TypeContext typeContext)
            {
                Value? capturedThis = null;
                if (lambdaDecl.CapturedStatement.ThisType != null)
                {
                    var appliedThisType = typeContext.Apply(lambdaDecl.CapturedStatement.ThisType);
                    capturedThis = evaluator.AllocValue(appliedThisType);
                }

                var capturesBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
                foreach (var (elemType, elemName) in lambdaDecl.CapturedStatement.OuterLocalVars)
                {
                    var appliedElemType = typeContext.Apply(elemType);

                    var elemValue = evaluator.AllocValue(appliedElemType);
                    capturesBuilder.Add(elemName, elemValue);
                }

                return new LambdaValue(capturedThis, capturesBuilder.ToImmutable());
            }

            public override async ValueTask InvokeAsync(
                Evaluator evaluator, 
                Value? capturedThis,
                ImmutableDictionary<string, Value> capturedVars,
                ImmutableDictionary<string, Value> localVars, 
                Value result)
            {
                await evaluator.context.ExecInNewFuncFrameAsync(capturedVars, localVars, EvalFlowControl.None, ImmutableArray<Task>.Empty, capturedThis, result, async () =>
                {
                    await foreach (var _ in evaluator.EvalStmtAsync(lambdaDecl.CapturedStatement.Body)) { }
                });
            }

            public override void Capture(Evaluator evaluator, LambdaValue lambdaValue)
            {
                evaluator.CaptureLocals(lambdaValue.CapturedThis, lambdaValue.Captures, lambdaDecl.CapturedStatement);
            }
        }
    }
}
