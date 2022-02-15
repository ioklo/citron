using Citron.Infra;
using Pretune;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Citron
{
    public interface ISequence
    {
        ValueTask<bool> MoveNextAsync(Value value);
    }

    // anonymous seq, supporting seq<T>
    [AutoConstructor, ImplementIEquatable]
    public partial class SeqValue : Value
    {
        [AllowNull]
        ISequence sequence;

        public SeqValue()
        {
            sequence = null;
        }

        public void SetSequence(ISequence sequence)
        {
            this.sequence = sequence;
        }        

        public override void SetValue(Value value)
        {
            // seq는 복사가 되지 않도록 Translator에서 체크해서 막아야 한다
            throw new RuntimeFatalException();
        }

        // 
        public ValueTask<bool> NextAsync(Value yieldValue)
        {
            Debug.Assert(sequence != null);
            return sequence.MoveNextAsync(yieldValue);
        }
    }
}