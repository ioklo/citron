using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0.Runtime
{
    public abstract class Value
    {
        public abstract void SetValue(Value value);
    }

    class IntValue : Value
    {
        int value;
        public IntValue() { this.value = 0; }

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
    }

    class BoolValue : Value
    {
        bool value;
        public BoolValue() { this.value = false; }
        public void SetBool(bool value) { this.value = value; }
        public bool GetBool() { return value; }

        public override void SetValue(Value fromValue)
        {
            value = ((BoolValue)fromValue).value;
        }
    }

    class StringValue : Value
    {
        string? value;

        public StringValue()
        {
            this.value = null;
        }

        public override void SetValue(Value fromValue)
        {
            value = ((StringValue)fromValue).value;
        }

        public string GetString()
        {
            return value!;
        }

        public void SetString(string value)
        {
            this.value = value;
        }
    }

    class AsyncEnumeratorValue : Value
    {
        IAsyncEnumerator<Value> enumerator;
        public AsyncEnumeratorValue(IAsyncEnumerator<Value> enumerator)
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
            enumerator = ((AsyncEnumeratorValue)fromValue).enumerator;
        }
    }

    class AsyncEnumerableValue : Value
    {
        IAsyncEnumerable<Value>? enumerable;        

        public override void SetValue(Value value)
        {
            enumerable = ((AsyncEnumerableValue)value).enumerable;
        }

        public void SetEnumerable(IAsyncEnumerable<Value> enumerable) { this.enumerable = enumerable; }

        public IAsyncEnumerable<Value> GetAsyncEnumerable()
        {
            return enumerable!;
        }
    }

    class VoidValue : Value
    {
        public static readonly Value Instance = new VoidValue();
        private VoidValue() { }

        public override void SetValue(Value value)
        {
            throw new NotImplementedException();
        }
    }

    class StructValue : Value
    {
        public override void SetValue(Value value)
        {
            throw new NotImplementedException();
        }

        public Value GetMemberValue(string name)
        {
            throw new NotImplementedException();
        }
    }

    class ClassValue : Value
    {
        public Value GetMemberValue(string name)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(Value value)
        {
            throw new NotImplementedException();
        }

        public new Type GetType()
        {
            throw new NotImplementedException();
        }
    }

    class EnumValue : Value
    {
        string elemName;
        ImmutableArray<NamedValue> members;

        public EnumValue()
        {
            elemName = string.Empty;
            members = ImmutableArray<NamedValue>.Empty;
        }

        public Value GetMemberValue(string name)
        {
            throw new NotImplementedException();

        }

        public override void SetValue(Value value)
        {
            throw new NotImplementedException();
        }

        public void SetEnum(string elemName, ImmutableArray<NamedValue> members)
        {
            this.elemName = elemName;
            this.members = members;
        }

        public string GetElemName()
        {
            return elemName;
        }
    }

    class ListValue : Value
    {
        List<Value>? list;
        
        public override void SetValue(Value value)
        {
            list = ((ListValue)value).list;
        }

        public void SetList(List<Value> list)
        {
            this.list = list;
        }

        public List<Value> GetList()
        {
            return list!;
        }
    }

    public class Lambda
    {
        public Value? CapturedThis { get; }                 // 캡쳐한 곳에 있던 this를 쓸지
        public ImmutableDictionary<string, Value> Captures { get; } 
        public ImmutableArray<string> ParamNames { get; }
        public Stmt Body { get; }

        public Lambda(Value? capturedThis, ImmutableDictionary<string, Value> captures, ImmutableArray<string> paramNames, Stmt body)
        {
            CapturedThis = capturedThis;
            Captures = captures;
            ParamNames = paramNames;
            Body = body;
        }
    }

    class LambdaValue : Value
    {
        Lambda? lambda;
        
        public LambdaValue()
        {
            lambda = null;
        }

        public Lambda GetLambda()
        {
            return lambda!;
        }
        
        public void SetLambda(Lambda lambda)
        {
            this.lambda = lambda;
        }

        public override void SetValue(Value value)
        {
            lambda = ((LambdaValue)value).lambda;
        }
    }

    class StaticValue : Value
    {
        public override void SetValue(Value value)
        {
            throw new NotImplementedException();
        }

        public Value GetMemberValue(string memberName)
        {
            throw new NotImplementedException();
        }
    }
}
