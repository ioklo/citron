using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.IR0Evaluator
{
    public struct EvalResult<TValue>
    {
        public static EvalResult<TValue> Invalid = new EvalResult<TValue>();

        public bool HasValue { get; }
        public TValue Value { get; }
        public EvalContext Context { get; }
        public EvalResult(TValue value, EvalContext context)
        {
            HasValue = true;
            Value = value;
            Context = context;
        }
    }
}
