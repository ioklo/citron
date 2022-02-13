using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class SeqFuncRuntimeItem : AllocatableRuntimeItem 
    {
        public abstract bool IsThisCall { get; }
        public abstract ImmutableArray<R.Param> Parameters { get; }

        public abstract void Invoke(Value? thisValue, ImmutableArray<Value> args, Value result);
    }

        [AutoConstructor]
        partial class IR0SeqFuncRuntimeItem : SeqFuncRuntimeItem
        {
            IR0GlobalContext globalContext;

            public override R.Name Name => seqFuncDecl.Name;
            public override R.ParamHash ParamHash => Misc.MakeParamHash(seqFuncDecl.TypeParams.Length, seqFuncDecl.Parameters);
            public override bool IsThisCall => seqFuncDecl.IsThisCall;
            public override ImmutableArray<R.Param> Parameters => seqFuncDecl.Parameters;
            R.SequenceFuncDecl seqFuncDecl;

            public override Value Alloc(TypeContext typeContext)
            {
                return new SeqValue();
            }

            public override void Invoke(Value? thisValue, ImmutableArray<Value> args, Value result)
            {
                var builder = ImmutableDictionary.CreateBuilder<R.Name, Value>();

                for (int i = 0; i < args.Length; i++)
                    builder.Add(seqFuncDecl.Parameters[i].Name, args[i]);

                var context = new IR0EvalContext(default, EvalFlowControl.None, thisValue, result);
                var localContext = new LocalContext(builder.ToImmutable());
                var localTaskContext = new LocalTaskContext();
                
                var enumerator = IR0StmtEvaluator.EvalAsyncEnum(globalContext, context, localContext, localTaskContext, seqFuncDecl.Body);
                
                ((SeqValue)result).SetEnumerator(enumerator, context);
            }
        }
}
