using Citron.CompileTime;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace Citron.Runtime
{
    // Internal Structure
    // qs type : c# type
    // null   : QsNullValue
    // int    : Value<int> 
    // bool   : Value<bool>
    // int &  : RefValue(Value<int>)
    // X &    : RefValue(Value) 
    // string : ObjectValue(QsStringObject) 
    // class T -> { type: typeInfo, ... } : QsObjectValue(QsClassObject) // 
    // { captures..., Invoke: func } 
    // () => { }

    // 값
    public abstract class Value
    {
        public abstract void SetValue(Value fromValue);
        public abstract Value MakeCopy();
    }

    public class LambdaValue : Value
    {
        public FuncInst? FuncInst { get; private set; }

        public LambdaValue()
        {
            FuncInst = null;
        }

        public LambdaValue(FuncInst? funcInst)
        {
            FuncInst = funcInst;
        }

        public void SetFuncInst(FuncInst funcInst)
        {
            FuncInst = funcInst;
        }
        
        public override void SetValue(Value fromValue)
        {
            FuncInst = ((LambdaValue)fromValue).FuncInst;
        }

        public override Value MakeCopy()
        {
            // ReferenceCopy
            return new LambdaValue(FuncInst);
        }
    }
    
    public class NullValue : Value
    {
        public static readonly NullValue Instance = new NullValue();
        private NullValue() { }

        public override void SetValue(Value fromValue)
        {
            if (!(fromValue is NullValue))
                throw new InvalidOperationException(); 
        }

        public override Value MakeCopy()
        {
            return Instance;
        }        
    }

    // void 
    public class VoidValue : Value
    {
        public static VoidValue Instance { get; } = new VoidValue();
        private VoidValue() { }        
        
        public override Value MakeCopy()
        {
            throw new InvalidOperationException();
        }

        public override void SetValue(Value fromValue)
        {
            throw new InvalidOperationException();
        }
    }

    public abstract class GumObject
    {
        public virtual TypeInst GetTypeInst()
        {
            throw new NotImplementedException();
        }

        public virtual Value GetMemberValue(Name varName)
        {
            throw new NotImplementedException();
        }

        protected static TObject GetObject<TObject>(Value value) where TObject : GumObject
        {
            return (TObject)((ObjectValue)value).Object!;
        }
    }    
   
    public class ObjectValue : Value
    {
        public GumObject? Object { get; private set; }

        public ObjectValue(GumObject? obj)
        {
            Object = obj;
        }
        
        public Value GetMemberValue(Name varName)
        {
            return Object!.GetMemberValue(varName);
        }

        public override Value MakeCopy()
        {
            return new ObjectValue(Object);
        }

        public override void SetValue(Value fromValue)
        {
            Object = ((ObjectValue)fromValue).Object;
        }

        public void SetObject(GumObject obj)
        {
            Object = obj;
        }

        public TypeInst GetTypeInst()
        {
            // 초기화 전에는 null일 수 있는데 타입체커를 통과하고 나면 호출하지 않을 것이다
            return Object!.GetTypeInst();
        }

        public override bool Equals(object? obj)
        {
            return obj is ObjectValue value &&
                   EqualityComparer<GumObject?>.Default.Equals(Object, value.Object);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Object);
        }

        public static bool operator ==(ObjectValue? left, ObjectValue? right)
        {
            return EqualityComparer<ObjectValue?>.Default.Equals(left, right);
        }

        public static bool operator !=(ObjectValue? left, ObjectValue? right)
        {
            return !(left == right);
        }
    }
    
}

