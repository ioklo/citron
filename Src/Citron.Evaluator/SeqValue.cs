using Gum.Infra;
using Pretune;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Void = Gum.Infra.Void;

namespace Citron
{
    [AutoConstructor, ImplementIEquatable]
    partial class SeqValue : Value
    {
        IAsyncEnumerator<Void>? enumerator;
        IR0EvalContext? context;

        public SeqValue()
        {
            enumerator = null;
            context = null;
        }

        public void SetEnumerator(IAsyncEnumerator<Void> enumerator, IR0EvalContext context)
        {
            this.enumerator = enumerator;
            this.context = context;
        }

        public override void SetValue(Value value)
        {
            // seq는 복사가 되지 않도록 Translator에서 체크해서 막아야 한다
            throw new RuntimeFatalException();
        }

        // 
        public ValueTask<bool> NextAsync(Value yieldValue)
        {
            Debug.Assert(enumerator != null && context != null);

            context.SetYieldValue(yieldValue);
            return enumerator.MoveNextAsync();
        }   
    }
}