using Pretune;
using System.Collections.Generic;

namespace Gum.IR0.Runtime
{
    [AutoConstructor, ImplementIEquatable]
    partial class SeqValue : Value
    {
        public IAsyncEnumerator<Value>? enumerator;
        public IAsyncEnumerator<Value> Enumerator
        {
            get => enumerator!;
            set { enumerator = value; }
        }

        public SeqValue()
        {
            enumerator = null;
        }

        public override void SetValue(Value value)
        {
            throw new System.NotImplementedException();
        }
    }
}