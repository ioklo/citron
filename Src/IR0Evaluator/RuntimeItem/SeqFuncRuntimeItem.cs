using Citron.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = Citron.IR0;

namespace Citron
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

        class SeqFuncSequence : ISequence
        {
            IAsyncEnumerator<Infra.Void> enumerator;
            IR0EvalContext evalContext;

            public SeqFuncSequence(IAsyncEnumerator<Infra.Void> enumerator, IR0EvalContext evalContext)
            {
                this.enumerator = enumerator;
                this.evalContext = evalContext;
            }

            public ValueTask<bool> MoveNextAsync(Value yieldValue)
            {
                evalContext.SetYieldValue(yieldValue);
                return enumerator.MoveNextAsync();
            }
        }

        public override void Invoke(Value? thisValue, ImmutableArray<Value> args, Value result)
        {
            var builder = ImmutableDictionary.CreateBuilder<R.Name, Value>();

            for (int i = 0; i < args.Length; i++)
                builder.Add(seqFuncDecl.Parameters[i].Name, args[i]);

            var context = new IR0EvalContext(default, IR0EvalFlowControl.None, thisValue, result);
            var localContext = new IR0LocalContext(builder.ToImmutable(), default);            
                
            var enumerator = IR0Evaluator.EvalAsyncEnum(globalContext, context, localContext, seqFuncDecl.Body);

            var sequence = new SeqFuncSequence(enumerator, context);
            ((SeqValue)result).SetSequence(sequence);
        }
    }
}
