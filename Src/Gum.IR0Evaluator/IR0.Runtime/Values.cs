using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using System.Threading.Tasks;
using Pretune;

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

    // ref<T>
    class RefValue : Value
    {
        public Value? Value { get; private set; }        
        public RefValue(Value value)
        {
            Value = value;
        }

        public override void SetValue(Value srcValue)
        {
            Value = ((RefValue)srcValue).Value;
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
    
    // 람다 호출시에 필요한 값들만 들고 있으면 된다
    class LambdaValue : Value
    {   
        public LambdaDeclId LambdaDeclId { get; }
        public Value? CapturedThis { get; }                 // 캡쳐한 곳에 있던 this를 쓸지, struct면 RefValue, boxed struct면 BoxValue, class 면 ClassValue
        public ImmutableDictionary<string, Value> Captures { get; }

        // AnonymousLambdaType으로 부터 Allocation을 해야 한다
        public LambdaValue(LambdaDeclId lambdaDeclId, Value? capturedThis, ImmutableDictionary<string, Value> captures)
        {
            LambdaDeclId = lambdaDeclId;
            CapturedThis = capturedThis;
            Captures = captures;
        }        
        
        // Copy의 성격이 더 가깝다
        public override void SetValue(Value srcValue)
        {
            LambdaValue srcLambdaValue = (LambdaValue)srcValue;

            if (CapturedThis != null)
                CapturedThis.SetValue(srcLambdaValue.CapturedThis!);

            foreach (var (name, value) in Captures)
                value.SetValue(srcLambdaValue.Captures[name]);
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
