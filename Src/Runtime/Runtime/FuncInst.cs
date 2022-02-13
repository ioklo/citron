using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Runtime
{
    public abstract class FuncInst
    {
        public abstract bool bThisCall { get; }
    }

    public class NativeFuncInst : FuncInst
    {
        public override bool bThisCall { get; }

        public delegate ValueTask InvokerDelegate(Value? thisValue, IReadOnlyList<Value> args, Value retValue);
        InvokerDelegate Invoker;

        public NativeFuncInst(bool bThisCall, InvokerDelegate Invoker)
        {
            this.bThisCall = bThisCall;
            this.Invoker = Invoker;
        }

        public ValueTask CallAsync(Value? thisValue, IReadOnlyList<Value> args, Value result)
        {
            Debug.Assert(bThisCall == (thisValue != null));
            return Invoker(thisValue, args, result);
        }
    }
}
