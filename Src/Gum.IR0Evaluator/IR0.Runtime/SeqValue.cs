using Pretune;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gum.IR0.Runtime
{
    [AutoConstructor, ImplementIEquatable]
    partial class SeqValue : Value
    {
        IAsyncEnumerator<Infra.Void>? enumerator;
        EvalContext? evalContext;

        public SeqValue()
        {
            enumerator = null;
        }

        public void SetEnumerator(IAsyncEnumerator<Infra.Void> enumerator, EvalContext evalContext)
        {
            this.enumerator = enumerator;
            this.evalContext = evalContext;
        }

        public override void SetValue(Value value)
        {
            // seq가 복사되면 어떻게 되나 +_+ 안되지
            // 
        }

        // 
        public ValueTask<bool> NextAsync(Value yieldValue)
        {
            Debug.Assert(enumerator != null && evalContext != null);

            evalContext.SetYieldValue(yieldValue);
            return enumerator.MoveNextAsync();
        }   
    }
}