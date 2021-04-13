using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Runtime
{
    class Value<TData> : Value where TData : struct
    {
        public TData Data { get; set; }
        public Value(TData data)
        {
            Data = data;
        }

        public override void SetValue(Value v)
        {
            Data = ((Value<TData>)v).Data;
        }

        public override Value MakeCopy()
        {
            return new Value<TData>(Data);
        }        
        
        public override bool Equals(object? obj)
        {
            return obj is Value<TData> value &&
                   EqualityComparer<TData>.Default.Equals(Data, value.Data);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Data);
        }

        public static bool operator ==(Value<TData>? left, Value<TData>? right)
        {
            return EqualityComparer<Value<TData>?>.Default.Equals(left, right);
        }

        public static bool operator !=(Value<TData>? left, Value<TData>? right)
        {
            return !(left == right);
        }
    }

}
