using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.Runtime
{
    public partial class IR0Evaluator
    {
        public class Frame
        {
            RefValue? retValueRef;
            ImmutableArray<Value> regValues;

            public Frame(RefValue? retValueRef, IEnumerable<Value> regValues)
            {
                this.retValueRef = retValueRef;
                this.regValues = regValues.ToImmutableArray();
            }

            public RefValue? GetRetValueRef()
            {
                return retValueRef;
            }

            public TValue GetRegValue<TValue>(RegId id) where TValue : Value
            {
                return (TValue)regValues[id.Value];
            }
        }   
    }
}
