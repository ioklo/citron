using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using System.Threading.Tasks;
using Pretune;

using System.Diagnostics;
using Citron.CompileTime;

namespace Citron
{
    public abstract class Value
    {
        public abstract void SetValue(Value value);
    }

    public class IntValue : Value
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

    public class BoolValue : Value
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
    public class EmptyValue : Value
    {
        public static readonly Value Instance = new EmptyValue();
        EmptyValue() { }

        public override void SetValue(Value value)
        {
            // SetValue를 먹는다
        }
    }

    public class StringValue : Value
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

    public class VoidValue : Value
    {
        public static readonly Value Instance = new VoidValue();
        private VoidValue() { }

        public override void SetValue(Value value)
        {
            throw new NotImplementedException();
        }
    }

    // ref T
    public class RefValue : Value
    {
        Value? value;

        public RefValue()
        {
            value = null;
        }

        public RefValue(Value value)
        {
            this.value = value;
        }

        public override void SetValue(Value srcValue)
        {
            // shallow copy
            this.value = ((RefValue)srcValue).value;
        }

        public void SetTarget(Value value)
        {
            this.value = value;
        }

        public Value GetTarget()
        {
            return value!;
        }
    }

    public class StructValue : Value
    {
        ImmutableDictionary<Name, Value> values;

        public StructValue(ImmutableDictionary<Name, Value> values)
        {
            this.values = values;
        }

        public override void SetValue(Value from_value)
        {
            StructValue from = (StructValue)from_value;

            foreach (var (name, value) in values)
                value.SetValue(from.values[name]);            
        }

        public Value GetMemberValue(Name name)
        {
            return values[name];
        }
    }    

    // vanilla implemenation    
    [AutoConstructor]
    public partial class ClassInstance
    {
        SymbolId classId;
        ImmutableArray<Value> values;

        public SymbolId GetActualType()
        {
            return classId;
        }

        public Value GetMemberValue(int index)
        {
            return values[index];
        }
    }
    
    public class ClassValue : Value
    {   
        ClassInstance? instance;

        public ClassValue()
        {
            instance = null;
        }

        public ClassValue(ClassInstance instance)
        {
            this.instance = instance;
        }

        public void SetInstance(ClassInstance instance)
        {
            this.instance = instance;
        }

        public ClassInstance GetInstance()
        {
            return this.instance!;
        }

        public override void SetValue(Value from_value)
        {
            ClassValue from = (ClassValue)from_value;
            this.instance = from.instance;
        }

        public SymbolId GetActualType()
        {
            return instance!.GetActualType();
        }
    }

    // E
    [AutoConstructor]
    public partial class EnumValue : Value
    {
        TypeContext typeContext;
        EnumElemRuntimeItem? enumElemItem;
        EnumElemValue? elemValue;        
        
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

        public bool IsElem(EnumElemRuntimeItem enumElemItem)
        {
            Debug.Assert(this.enumElemItem != null);

            return this.enumElemItem.Equals(enumElemItem); // reference 비교 가능하도록, 불가능 하면 R.EnumElement를 쓰지 말고 동적으로 생성되는 타입을 하나 만든다
        }

        public void SetEnumElemItem(EnumElemRuntimeItem enumElemItem)
        {
            if (EqualityComparer<EnumElemRuntimeItem>.Default.Equals(this.enumElemItem, enumElemItem))
                return;

            this.enumElemItem = enumElemItem;
            Debug.Assert(enumElemItem != null);
            elemValue = (EnumElemValue)enumElemItem.Alloc(typeContext);
        }

        public EnumElemValue GetElemValue()
        {
            return elemValue!;
        }
    }

    // E.First
    [AutoConstructor]
    public partial class EnumElemValue : Value
    {
        public ImmutableArray<Value> Fields { get; }

        public override void SetValue(Value value)
        {
            var enumElemValue = (EnumElemValue)value;
            for(int i = 0; i < Fields.Length; i++)
                Fields[i].SetValue(enumElemValue.Fields[i]);
        }
    }

    public class ListValue : Value
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
    public class TupleValue : Value
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
    public class LambdaValue : Value
    {
        ImmutableDictionary<Name, Value> memberVars;

        // AnonymousLambdaType으로 부터 Allocation을 해야 한다
        public LambdaValue(ImmutableDictionary<Name, Value> memberVars)
        {
            this.memberVars = memberVars;
        }        
        
        // memberwise copy의 성격이 더 가깝다
        public override void SetValue(Value srcValue)
        {
            LambdaValue srcLambdaValue = (LambdaValue)srcValue;

            foreach (var (name, value) in memberVars)
                value.SetValue(srcLambdaValue.memberVars[name]);
        }

        public Value GetMemberValue(Name name)
        {
            return memberVars[name];
        }
    }

    public class StaticValue : Value
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
