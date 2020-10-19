using Gum.StaticAnalysis;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Gum.CompileTime;

namespace Gum
{   
    public class ListExpInfo : SyntaxNodeInfo
    {
        public TypeValue ElemTypeValue { get; }
        public ListExpInfo(TypeValue elemTypeValue) { ElemTypeValue = elemTypeValue; }
    }

    public class MemberExpInfo : SyntaxNodeInfo
    {
        public class Instance : MemberExpInfo
        {
            public TypeValue ObjectTypeValue { get; }
            public Name VarName { get; }

            public Instance(TypeValue objectTypeValue, Name varName)
            {
                ObjectTypeValue = objectTypeValue;
                VarName = varName;
            }
        }

        public class Static : MemberExpInfo
        {
            public bool bEvaluateObject => ObjectTypeValue != null;
            public TypeValue? ObjectTypeValue { get; }
            public VarValue VarValue { get; }

            public Static(TypeValue? objectTypeValue, VarValue varValue)
            {
                ObjectTypeValue = objectTypeValue;
                VarValue = varValue;
            }
        }

        // E.One
        public class EnumElem : MemberExpInfo
        {
            public TypeValue.Normal EnumTypeValue { get; }
            public string Name { get; }

            public EnumElem(TypeValue.Normal enumTypeValue, string name)
            {
                EnumTypeValue = enumTypeValue;
                Name = name;
            }
        }

        // e.i
        public class EnumElemField : MemberExpInfo
        {
            public TypeValue.Normal ObjectTypeValue;
            public string Name { get; }

            public EnumElemField(TypeValue.Normal objTypeValue, string name)
            {
                ObjectTypeValue = objTypeValue;
                Name = name;
            }
        }

        public static MemberExpInfo MakeInstance(TypeValue objectTypeValue, Name varName) 
            => new Instance(objectTypeValue, varName);

        public static MemberExpInfo MakeStatic(TypeValue? objectTypeValue, VarValue varValue)
            => new Static(objectTypeValue, varValue);

        public static MemberExpInfo MakeEnumElem(TypeValue.Normal enumTypeValue, string elemName)
            => new EnumElem(enumTypeValue, elemName);

        public static MemberExpInfo MakeEnumElemField(TypeValue.Normal objTypeValue, string name)
            => new EnumElemField(objTypeValue, name);
    }

    public class BinaryOpExpInfo : SyntaxNodeInfo
    {
        public enum OpType
        {
            Bool, Integer, String
        }

        public OpType Type { get; }
        public BinaryOpExpInfo(OpType type)
        {
            Type = type;
        }
    }

    public class BinaryOpExpAssignInfo : SyntaxNodeInfo
    {
        public class Direct : BinaryOpExpAssignInfo        
        {
            public StorageInfo StorageInfo { get; }
            public Direct(StorageInfo storageInfo) { StorageInfo = storageInfo; }
        }
        
        public class CallSetter : BinaryOpExpAssignInfo
        {
            public TypeValue? ObjectTypeValue { get; }
            public Exp? Object { get;}            
            public FuncValue Setter { get;}
            public ImmutableArray<(Exp Exp, TypeValue TypeValue)> Arguments { get; }
            public TypeValue ValueTypeValue { get; }

            public CallSetter(
                TypeValue? objTypeValue,
                Exp? obj,
                FuncValue setter,
                IEnumerable<(Exp Exp, TypeValue TypeValue)> arguments,
                TypeValue valueTypeValue)
            {
                ObjectTypeValue = objTypeValue;
                Object = obj;                
                Setter = setter;
                Arguments = arguments.ToImmutableArray();
                ValueTypeValue = valueTypeValue;
            }
        }

        public static Direct MakeDirect(StorageInfo storageInfo) 
            => new Direct(storageInfo);

        public static CallSetter MakeCallSetter(
            TypeValue? objTypeValue,
            Exp? obj,
            FuncValue setter,
            IEnumerable<(Exp Exp, TypeValue TypeValue)> arguments,
            TypeValue valueTypeValue)
            => new CallSetter(objTypeValue, obj, setter, arguments, valueTypeValue);
    }

    //public class UnaryOpExpAssignInfo : SyntaxNodeInfo
    //{
    //    public class Direct : UnaryOpExpAssignInfo
    //    {
    //        public StorageInfo StorageInfo { get; }
    //        public FuncValue OperatorValue { get; }
    //        public bool bReturnPrevValue { get; }
    //        public TypeValue ValueTypeValue { get; }

    //        public Direct(StorageInfo storageInfo, FuncValue operatorValue, bool bReturnPrevValue, TypeValue valueTypeValue) 
    //        { 
    //            StorageInfo = storageInfo;
    //            OperatorValue = operatorValue;
    //            this.bReturnPrevValue = bReturnPrevValue;
    //            ValueTypeValue = valueTypeValue;
    //        }
    //    }

    //    public class CallFunc : UnaryOpExpAssignInfo
    //    {
    //        public Exp? ObjectExp { get; }
    //        public TypeValue? ObjectTypeValue { get; }

    //        public TypeValue ValueTypeValue0 { get; }
    //        public TypeValue ValueTypeValue1 { get; }
    //        public bool bReturnPrevValue { get; }

    //        // Getter/setter Arguments without setter value
    //        public ImmutableArray<(Exp Exp, TypeValue TypeValue)> Arguments { get; }

    //        public FuncValue Getter { get; }            
    //        public FuncValue Setter { get; }
    //        public FuncValue Operator { get; }

    //        public CallFunc(
    //            Exp? objectExp,
    //            TypeValue? objectTypeValue,
    //            TypeValue valueTypeValue0,
    //            TypeValue valueTypeValue1,
    //            bool bReturnPrevValue,
    //            IEnumerable<(Exp Exp, TypeValue TypeValue)> arguments,
    //            FuncValue getter,
    //            FuncValue setter,
    //            FuncValue op)
    //        {
    //            ObjectExp = objectExp;
    //            ObjectTypeValue = objectTypeValue;
    //            ValueTypeValue0 = valueTypeValue0;
    //            ValueTypeValue1 = valueTypeValue1;
    //            this.bReturnPrevValue= bReturnPrevValue;

    //            Arguments = arguments.ToImmutableArray();
    //            Getter = getter;
    //            Setter = setter;
    //            Operator = op;
    //        }
    //    }

    //    public static Direct MakeDirect(StorageInfo storageInfo, FuncValue operatorValue, bool bReturnPrevValue, TypeValue valueTypeValue)
    //        => new Direct(storageInfo, operatorValue, bReturnPrevValue, valueTypeValue);

    //    public static CallFunc MakeCallFunc(
    //        Exp? objectExp,
    //        TypeValue? objectTypeValue,
    //        TypeValue valueTypeValue0,
    //        TypeValue valueTypeValue1,
    //        bool bReturnPrevValue,
    //        IEnumerable<(Exp Exp, TypeValue TypeValue)> arguments,
    //        FuncValue getter,
    //        FuncValue setter,
    //        FuncValue op)
    //        => new CallFunc(objectExp, objectTypeValue, valueTypeValue0, valueTypeValue1, bReturnPrevValue, arguments, getter, setter, op);
    //}

    //public class MemberCallExpInfo : SyntaxNodeInfo
    //{
    //    public TypeValue? ObjectTypeValue { get; }
    //    public ImmutableArray<TypeValue> ArgTypeValues { get; set; }

    //    // C.F(), x.F() // F is static
    //    public class StaticFuncCall : MemberCallExpInfo
    //    {
    //        public bool bEvaluateObject { get => ObjectTypeValue != null; }
    //        public FuncValue FuncValue { get; }

    //        public StaticFuncCall(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, FuncValue funcValue)
    //            : base(objectTypeValue, argTypeValues)
    //        {
    //            FuncValue = funcValue;
    //        }
    //    }

    //    // x.F()
    //    public class InstanceFuncCall : MemberCallExpInfo
    //    {
    //        public FuncValue FuncValue { get; }
    //        public InstanceFuncCall(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, FuncValue funcValue)
    //            : base(objectTypeValue, argTypeValues)
    //        {
    //            FuncValue = funcValue;
    //        }
    //    }

    //    // x.f() C.f()
    //    public class InstanceLambdaCall : MemberCallExpInfo
    //    {
    //        public Name VarName { get; }
    //        public InstanceLambdaCall(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, Name varName)
    //            : base(objectTypeValue, argTypeValues)
    //        {
    //            VarName = varName;
    //        }
    //    }

    //    public class StaticLambdaCall : MemberCallExpInfo
    //    {
    //        public bool bEvaluateObject { get => ObjectTypeValue != null; }
    //        public VarValue VarValue { get; }
    //        public StaticLambdaCall(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, VarValue varValue)
    //            : base(objectTypeValue, argTypeValues)
    //        {
    //            VarValue = varValue;
    //        }
    //    }

    //    public class EnumValue : MemberCallExpInfo
    //    {
    //        public EnumElemInfo ElemInfo { get; }
    //        public EnumValue(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, EnumElemInfo elemInfo)
    //            : base(objectTypeValue, argTypeValues)
    //        {
    //            ElemInfo = elemInfo;
    //        }
    //    }

    //    // 네개 씩이나 나눠야 하다니
    //    public static MemberCallExpInfo MakeStaticFunc(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, FuncValue funcValue)
    //        => new StaticFuncCall(objectTypeValue, argTypeValues, funcValue);

    //    public static MemberCallExpInfo MakeInstanceFunc(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, FuncValue funcValue)
    //        => new InstanceFuncCall(objectTypeValue, argTypeValues, funcValue);

    //    public static MemberCallExpInfo MakeStaticLambda(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, VarValue varValue)
    //        => new StaticLambdaCall(objectTypeValue, argTypeValues, varValue);

    //    public static MemberCallExpInfo MakeInstanceLambda(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, Name varName)
    //        => new InstanceLambdaCall(objectTypeValue, argTypeValues, varName);

    //    public static MemberCallExpInfo MakeEnumValue(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, EnumElemInfo elemInfo)
    //        => new EnumValue(objectTypeValue, argTypeValues, elemInfo);

    //    // 왜 private 인데 base()가 먹는지;
    //    private MemberCallExpInfo(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues)
    //    {
    //        ObjectTypeValue = objectTypeValue;
    //        ArgTypeValues = argTypeValues;
    //    }
    //}    
}
