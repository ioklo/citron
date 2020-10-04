using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;

namespace Gum.Runtime
{
    public partial class IR1Evaluator
    {
        public class RefValue : Value
        {
            public Value? target;

            public RefValue(Value? target)
            {
                this.target = target;
            }

            public override Value MakeCopy()
            {
                return new RefValue(target);
            }

            public override void SetValue(Value fromValue)
            {
                target = ((RefValue)fromValue).target;
            }

            public Value GetTarget()
            {
                return target!;
            }

            public void SetTarget(Value target)
            {
                this.target = target;
            }
        }

        public class CompValue : Value
        {
            ImmutableArray<Value> values;

            public CompValue(IEnumerable<Value> values)
            {
                this.values = values.ToImmutableArray();
            }

            public Value GetValue(int index)
            {
                return values[index];
            }

            public override Value MakeCopy()
            {
                return new CompValue(values.Select(value => value.MakeCopy()));
            }

            public override void SetValue(Value fromValue)
            {
                var fromValues = ((CompValue)fromValue).values;

                for (int i = 0; i < values.Length; i++)
                    values[i].SetValue(fromValues[i]);
            }
        }

        public class IntValue : Value
        {
            int value;
            public IntValue(int value) { this.value = value; }

            public int GetInt()
            {
                return value;
            }

            public void SetInt(int value)
            {
                this.value = value;
            }

            public override void SetValue(Value fromValue)
            {
                value = ((IntValue)fromValue).value;
            }

            public override Value MakeCopy() { throw new NotImplementedException(); }
        }

        public class BoolValue : Value
        {
            bool value;
            public BoolValue(bool value) { this.value = value; }
            public void SetBool(bool value) { this.value = value; }
            public bool GetBool() { return value; }

            public override void SetValue(Value fromValue)
            {
                value = ((BoolValue)fromValue).value;
            }

            public override Value MakeCopy() { throw new NotImplementedException(); }
        }
        
        public class StringValue : Value
        {
            string value;
            public StringValue(string value)
            {
                this.value = value;
            }

            public override Value MakeCopy()
            {
                return new StringValue(value);
            }

            public override void SetValue(Value fromValue)
            {
                ((StringValue)fromValue).value = value;
            }

            public string GetString()
            {
                return value;
            }
        }

        public class EnumeratorValue : Value
        {
            IAsyncEnumerator<Value> enumerator;
            public EnumeratorValue(IAsyncEnumerator<Value> enumerator)
            {
                this.enumerator = enumerator;
            }

            public IAsyncEnumerator<Value> GetEnumerator()
            {
                return enumerator;
            }

            public void SetEnumerator(IAsyncEnumerator<Value> asyncEnum)
            {
                this.enumerator = asyncEnum;
            }

            public override void SetValue(Value fromValue)
            {
                enumerator = ((EnumeratorValue)fromValue).enumerator;
            }

            public override Value MakeCopy() { throw new NotImplementedException(); }
        }
    }
}
