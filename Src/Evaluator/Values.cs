using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using System.Threading.Tasks;
using Pretune;

using System.Diagnostics;
using Citron.Symbol;

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
    
    public class StructValue : Value
    {
        ImmutableArray<Value> values;        

        public StructValue(ImmutableArray<Value> values)
        {
            this.values = values;
        }

        public override void SetValue(Value from_value)
        {
            StructValue from = (StructValue)from_value;

            Debug.Assert(from.values.Length == this.values.Length);

            for (int i = 0; i < values.Length; i++)
                values[i].SetValue(from.values[i]);           
        }

        public Value GetMemberValue(int index)
        {
            return values[index];
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
    public class EnumValue : Value
    {
        // 이 implementation에서는 EnumElem에 해당하는 모든 Value를 다 할당한다
        Func<SymbolId, EnumElemValue> elemAllocator; // lazy allocation
        SymbolId? elemId;
        Dictionary<SymbolId, EnumElemValue> elems;

        public EnumValue(Func<SymbolId, EnumElemValue> elemAllocator, SymbolId? elemId)
        {
            this.elemAllocator = elemAllocator;
            this.elemId = elemId;
            this.elems = new Dictionary<SymbolId, EnumElemValue>();
        }

        // E e1, e2;
        // e1 = e2;
        public override void SetValue(Value src_value)
        {
            var src = (EnumValue)src_value;

            Debug.Assert(src.elemId != null);
            this.elemId = src.elemId;

            var elem = GetElemValue();
            var srcElem = src.GetElemValue();
            elem.SetValue(srcElem);
        }

        public bool IsElem(SymbolId elemId)
        {
            if (this.elemId == null)
                return false;

            return this.elemId.Equals(elemId); // reference 비교 가능하도록, 불가능 하면 R.EnumElement를 쓰지 말고 동적으로 생성되는 타입을 하나 만든다
        }

        public void SetEnumElemId(SymbolId enumElemId)
        {
            if (EqualityComparer<SymbolId>.Default.Equals(this.elemId, enumElemId))
                return;

            this.elemId = enumElemId;
        }

        public EnumElemValue GetElemValue()
        {
            Debug.Assert(elemId != null);

            if (!elems.TryGetValue(elemId, out var elem))
            {
                elem = elemAllocator.Invoke(elemId);
                elems.Add(elemId, elem);
            }

            return elem;
        }
    }

    // E.First
    [AutoConstructor]
    public partial class EnumElemValue : Value
    {
        ImmutableArray<Value> memberValues;

        public override void SetValue(Value value)
        {
            var enumElemValue = (EnumElemValue)value;
            for(int i = 0; i < memberValues.Length; i++)
                memberValues[i].SetValue(enumElemValue.memberValues[i]);
        }

        public Value GetMemberValue(int index)
        {
            return memberValues[index];
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

        public void Update(Name name, Value value)
        {
            memberVars[name].SetValue(value);
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
    
    public class BoxPtrValue : Value
    {
        Value? holder;
        Value? target;

        public Value GetTarget()
        {
            return this.target!;
        }
        
        public void Set(Value? holder, Value target)
        {
            this.holder = holder;
            this.target = target;
        }

        public void SetTarget(Value target)
        {
            this.target = target;
        }

        public override void SetValue(Value value)
        {
            this.target = ((BoxPtrValue)value).target;
        }
    }

    public class LocalPtrValue : Value
    {
        Value? target;

        public Value GetTarget()
        {
            return target!;
        }

        public void SetTarget(Value target)
        {
            this.target = target;
        }

        public override void SetValue(Value value)
        {
            this.target = ((LocalPtrValue)value).target;
        }
    }
}
