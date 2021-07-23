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
        public abstract ImmutableArray<R.Param> Parameters { get; }
        
        public abstract ValueTask InvokeAsync(Value? capturedThis, ImmutableDictionary<string, Value> capturedVars, ImmutableArray<Value> args, Value result);
        public abstract void Capture(EvalContext context, LocalContext localContext, LambdaValue lambdaValue);
    }

    public partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0LambdaRuntimeItem : LambdaRuntimeItem
        {
            GlobalContext globalContext;

            public override R.Name Name => lambdaDecl.Name;
            public override R.ParamHash ParamHash => R.ParamHash.None;
            public override ImmutableArray<R.Param> Parameters => lambdaDecl.Parameters;
            R.LambdaDecl lambdaDecl;

            public override Value Alloc(TypeContext typeContext)
            {
                Value? capturedThis = null;
                if (lambdaDecl.CapturedStatement.ThisType != null)
                {
                    var appliedThisType = typeContext.Apply(lambdaDecl.CapturedStatement.ThisType);
                    capturedThis = globalContext.AllocValue(appliedThisType);
                }

                var capturesBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
                foreach (var (elemType, elemName) in lambdaDecl.CapturedStatement.OuterLocalVars)
                {
                    var appliedElemType = typeContext.Apply(elemType);

                    var elemValue = globalContext.AllocValue(appliedElemType);
                    capturesBuilder.Add(elemName, elemValue);
                }

                return new LambdaValue(capturedThis, capturesBuilder.ToImmutable());
            }

            public override ValueTask InvokeAsync(
                Value? capturedThis,
                ImmutableDictionary<string, Value> capturedVars,
                ImmutableArray<Value> args, 
                Value result)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Value>();

                for (int i = 0; i < args.Length; i++)
                    builder.Add(lambdaDecl.Parameters[i].Name, args[i]);

                var context = new EvalContext(capturedVars, EvalFlowControl.None, capturedThis, result);
                var localContext = new LocalContext(builder.ToImmutable());
                var localTaskContext = new LocalTaskContext();

                return StmtEvaluator.EvalAsync(globalContext, context, localContext, localTaskContext, lambdaDecl.CapturedStatement.Body);
            }

            public override void Capture(EvalContext context, LocalContext localContext, LambdaValue lambdaValue)
            {
                Evaluator.CaptureLocals(context, localContext, lambdaValue.CapturedThis, lambdaValue.Captures, lambdaDecl.CapturedStatement);
            }
        }
    }
}
