using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using System.Threading.Tasks;
using Pretune;

using R = Gum.IR0;
using System.Diagnostics;

namespace Gum.IR0Evaluator
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

    // 특수 Value
    class EmptyValue : Value
    {
        public static readonly Value Instance = new EmptyValue();
        EmptyValue() { }

        public override void SetValue(Value value)
        {
            // SetValue를 먹는다
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

        public new R.Path GetType()
        {
            throw new NotImplementedException();
        }
    }

    // E
    [AutoConstructor]
    partial class EnumValue : Value
    {
        Evaluator evaluator;
        TypeContext typeContext;
        EnumElemRuntimeItem? enumElemItem;
        EnumElemValue? elemValue;
        
        public Value GetMemberValue(string name)
        {
            throw new NotImplementedException();
        }

        // E e1, e2;
        // e1 = e2;
        public override void SetValue(Value value_value)
        {
            var value = (EnumValue)value_value;

            Debug.Assert(value.enumElemItem != null);
            SetEnumElemItem(value.enumElemItem);

            Debug.Assert(elemValue != null && value.elemValue != null);
            elemValue.SetValue(value.elemValue);
        }

        public bool IsElem(EnumElemRuntimeItem EnumElemItem)
        {
            return EnumElemItem.Equals(EnumElemItem); // reference 비교 가능하도록, 불가능 하면 R.EnumElement를 쓰지 말고 동적으로 생성되는 타입을 하나 만든다
        }

        public void SetEnumElemItem(EnumElemRuntimeItem enumElemItem)
        {
            if (EqualityComparer<EnumElemRuntimeItem>.Default.Equals(this.enumElemItem, enumElemItem))
                return;

            this.enumElemItem = enumElemItem;
            Debug.Assert(enumElemItem != null);
            elemValue = (EnumElemValue)enumElemItem.Alloc(evaluator, typeContext);
        }

        public EnumElemValue GetElemValue()
        {
            return elemValue!;
        }
    }

    // E.First
    [AutoConstructor]
    partial class EnumElemValue : Value
    {
        public ImmutableArray<Value> Fields { get; }

        public override void SetValue(Value value)
        {
            var enumElemValue = (EnumElemValue)value;
            for(int i = 0; i < Fields.Length; i++)
                Fields[i].SetValue(enumElemValue.Fields[i]);
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

    // 
    class TupleValue : Value
    {
        public ImmutableArray<Value> ElemValues { get; }

        public TupleValue(ImmutableArray<Value> elemValues)
        {
            ElemValues = elemValues;
        }

        public override void SetValue(Value value)
        {
            var tupleValue = ((TupleValue)value);
            for (int i = 0; i < ElemValues.Length; i++)
                ElemValues[i].SetValue(tupleValue.ElemValues[i]);
        }
    }
    
    // 람다 호출시에 필요한 값들만 들고 있으면 된다
    class LambdaValue : Value
    {
        public Value? CapturedThis { get; }                 // 캡쳐한 곳에 있던 this를 쓸지, struct면 RefValue, boxed struct면 BoxValue, class 면 ClassValue
        public ImmutableDictionary<string, Value> Captures { get; }

        // AnonymousLambdaType으로 부터 Allocation을 해야 한다
        public LambdaValue(Value? capturedThis, ImmutableDictionary<string, Value> captures)
        {
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
