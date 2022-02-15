using Citron.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Citron.IR0;

namespace Citron
{
    abstract class LambdaRuntimeItem : AllocatableRuntimeItem
    {
        public abstract ImmutableArray<R.Param> Parameters { get; }
        
        public abstract ValueTask InvokeAsync(Value? capturedThis, ImmutableDictionary<string, Value> capturedVars, ImmutableArray<Value> args, Value result);
        public abstract void Capture(IR0EvalContext context, IR0LocalContext localContext, LambdaValue lambdaValue);
    }

        [AutoConstructor]
        partial class IR0LambdaRuntimeItem : LambdaRuntimeItem
        {
            IR0GlobalContext globalContext;

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
                    capturedThis = context.AllocValue(appliedThisType);
                }

                var capturesBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
                foreach (var (elemType, elemName) in lambdaDecl.CapturedStatement.OuterLocalVars)
                {
                    var appliedElemType = typeContext.Apply(elemType);

                    var elemValue = context.AllocValue(appliedElemType);
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
                
            }

            public override void Capture(IR0EvalContext context, IR0LocalContext localContext, LambdaValue lambdaValue)
            {
                Evaluator.CaptureLocals(context, localContext, lambdaValue.CapturedThis, lambdaValue.Captures, lambdaDecl.CapturedStatement);
            }
        }
}
