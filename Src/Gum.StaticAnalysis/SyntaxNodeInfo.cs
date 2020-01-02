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
    public abstract class SyntaxNodeInfo
    {
    }
    
    public class IdentifierExpInfo : SyntaxNodeInfo
    {
        public StorageInfo StorageInfo { get; }
        public IdentifierExpInfo(StorageInfo storageInfo) { StorageInfo = storageInfo; }
    }

    public class LambdaExpInfo : SyntaxNodeInfo
    {   
        public CaptureInfo CaptureInfo { get; }
        public int LocalVarCount { get; }

        public LambdaExpInfo(CaptureInfo captureInfo, int localVarCount)
        {
            CaptureInfo = captureInfo;
            LocalVarCount = localVarCount;
        }        
    }

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

    public class UnaryOpExpAssignInfo : SyntaxNodeInfo
    {
        public class Direct : UnaryOpExpAssignInfo
        {
            public StorageInfo StorageInfo { get; }
            public FuncValue OperatorValue { get; }
            public bool bReturnPrevValue { get; }
            public TypeValue ValueTypeValue { get; }

            public Direct(StorageInfo storageInfo, FuncValue operatorValue, bool bReturnPrevValue, TypeValue valueTypeValue) 
            { 
                StorageInfo = storageInfo;
                OperatorValue = operatorValue;
                this.bReturnPrevValue = bReturnPrevValue;
                ValueTypeValue = valueTypeValue;
            }
        }

        public class CallFunc : UnaryOpExpAssignInfo
        {
            public Exp? ObjectExp { get; }
            public TypeValue? ObjectTypeValue { get; }

            public TypeValue ValueTypeValue0 { get; }
            public TypeValue ValueTypeValue1 { get; }
            public bool bReturnPrevValue { get; }

            // Getter/setter Arguments without setter value
            public ImmutableArray<(Exp Exp, TypeValue TypeValue)> Arguments { get; }

            public FuncValue Getter { get; }            
            public FuncValue Setter { get; }
            public FuncValue Operator { get; }

            public CallFunc(
                Exp? objectExp,
                TypeValue? objectTypeValue,
                TypeValue valueTypeValue0,
                TypeValue valueTypeValue1,
                bool bReturnPrevValue,
                IEnumerable<(Exp Exp, TypeValue TypeValue)> arguments,
                FuncValue getter,
                FuncValue setter,
                FuncValue op)
            {
                ObjectExp = objectExp;
                ObjectTypeValue = objectTypeValue;
                ValueTypeValue0 = valueTypeValue0;
                ValueTypeValue1 = valueTypeValue1;
                this.bReturnPrevValue= bReturnPrevValue;

                Arguments = arguments.ToImmutableArray();
                Getter = getter;
                Setter = setter;
                Operator = op;
            }
        }

        public static Direct MakeDirect(StorageInfo storageInfo, FuncValue operatorValue, bool bReturnPrevValue, TypeValue valueTypeValue)
            => new Direct(storageInfo, operatorValue, bReturnPrevValue, valueTypeValue);

        public static CallFunc MakeCallFunc(
            Exp? objectExp,
            TypeValue? objectTypeValue,
            TypeValue valueTypeValue0,
            TypeValue valueTypeValue1,
            bool bReturnPrevValue,
            IEnumerable<(Exp Exp, TypeValue TypeValue)> arguments,
            FuncValue getter,
            FuncValue setter,
            FuncValue op)
            => new CallFunc(objectExp, objectTypeValue, valueTypeValue0, valueTypeValue1, bReturnPrevValue, arguments, getter, setter, op);
    }

    public abstract class CallExpInfo : SyntaxNodeInfo
    {
        public class Normal : CallExpInfo
        {
            public FuncValue? FuncValue { get; }
            public ImmutableArray<TypeValue> ArgTypeValues { get; }

            public Normal(FuncValue? funcValue, IEnumerable<TypeValue> argTypeValues)
            {
                FuncValue = funcValue;
                ArgTypeValues = argTypeValues.ToImmutableArray();
            }
        }

        public class EnumValue : CallExpInfo
        {
            public  EnumElemInfo ElemInfo { get; }
            public ImmutableArray<TypeValue> ArgTypeValues { get; }

            public EnumValue(EnumElemInfo elemInfo, IEnumerable<TypeValue> argTypeValues)
            {
                ElemInfo = elemInfo;
                ArgTypeValues = argTypeValues.ToImmutableArray();
            }
        }

        public static Normal MakeNormal(FuncValue? funcValue, ImmutableArray<TypeValue> argTypeValues) => new Normal(funcValue, argTypeValues);
        public static EnumValue MakeEnumValue(EnumElemInfo elemInfo, IEnumerable<TypeValue> argTypeValues) => new EnumValue(elemInfo, argTypeValues);
    }

    public class MemberCallExpInfo : SyntaxNodeInfo
    {
        public TypeValue? ObjectTypeValue { get; }
        public ImmutableArray<TypeValue> ArgTypeValues { get; set; }

        // C.F(), x.F() // F is static
        public class StaticFuncCall : MemberCallExpInfo
        {
            public bool bEvaluateObject { get => ObjectTypeValue != null; }
            public FuncValue FuncValue { get; }

            public StaticFuncCall(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, FuncValue funcValue)
                : base(objectTypeValue, argTypeValues)
            {
                FuncValue = funcValue;
            }
        }

        // x.F()
        public class InstanceFuncCall : MemberCallExpInfo
        {
            public FuncValue FuncValue { get; }
            public InstanceFuncCall(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, FuncValue funcValue)
                : base(objectTypeValue, argTypeValues)
            {
                FuncValue = funcValue;
            }
        }

        // x.f() C.f()
        public class InstanceLambdaCall : MemberCallExpInfo
        {
            public Name VarName { get; }
            public InstanceLambdaCall(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, Name varName)
                : base(objectTypeValue, argTypeValues)
            {
                VarName = varName;
            }
        }

        public class StaticLambdaCall : MemberCallExpInfo
        {
            public bool bEvaluateObject { get => ObjectTypeValue != null; }
            public VarValue VarValue { get; }
            public StaticLambdaCall(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, VarValue varValue)
                : base(objectTypeValue, argTypeValues)
            {
                VarValue = varValue;
            }
        }

        public class EnumValue : MemberCallExpInfo
        {
            public EnumElemInfo ElemInfo { get; }
            public EnumValue(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, EnumElemInfo elemInfo)
                : base(objectTypeValue, argTypeValues)
            {
                ElemInfo = elemInfo;
            }
        }

        // 네개 씩이나 나눠야 하다니
        public static MemberCallExpInfo MakeStaticFunc(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, FuncValue funcValue)
            => new StaticFuncCall(objectTypeValue, argTypeValues, funcValue);

        public static MemberCallExpInfo MakeInstanceFunc(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, FuncValue funcValue)
            => new InstanceFuncCall(objectTypeValue, argTypeValues, funcValue);

        public static MemberCallExpInfo MakeStaticLambda(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, VarValue varValue)
            => new StaticLambdaCall(objectTypeValue, argTypeValues, varValue);

        public static MemberCallExpInfo MakeInstanceLambda(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, Name varName)
            => new InstanceLambdaCall(objectTypeValue, argTypeValues, varName);

        public static MemberCallExpInfo MakeEnumValue(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues, EnumElemInfo elemInfo)
            => new EnumValue(objectTypeValue, argTypeValues, elemInfo);

        // 왜 private 인데 base()가 먹는지;
        private MemberCallExpInfo(TypeValue? objectTypeValue, ImmutableArray<TypeValue> argTypeValues)
        {
            ObjectTypeValue = objectTypeValue;
            ArgTypeValues = argTypeValues;
        }
    }

    public class ExpStmtInfo : SyntaxNodeInfo
    {
        public TypeValue ExpTypeValue { get; }
        public ExpStmtInfo(TypeValue expTypeValue)
        {
            ExpTypeValue = expTypeValue;
        }
    }

    public class VarDeclInfo : SyntaxNodeInfo
    {
        public class Element
        {
            public TypeValue TypeValue { get; }
            public StorageInfo StorageInfo { get; }
            
            public Element(TypeValue typeValue, StorageInfo storageInfo)
            {
                TypeValue = typeValue;
                StorageInfo = storageInfo;
            }
        }
        
        public ImmutableArray<Element> Elems;

        public VarDeclInfo(ImmutableArray<Element> elems)
        {
            Elems = elems;
        }
    }

    public abstract class IfStmtInfo : SyntaxNodeInfo
    {
        public class TestEnum : IfStmtInfo
        {
            public TypeValue TestTargetTypeValue { get; }
            public string ElemName { get; }

            public TestEnum(TypeValue testTargetTypeValue, string elemName)
            {
                TestTargetTypeValue = testTargetTypeValue;
                ElemName = elemName;
            }
        }

        public class TestClass : IfStmtInfo
        {
            public TypeValue TestTargetTypeValue { get; }
            public TypeValue TestTypeValue { get; }

            public TestClass(TypeValue testTargetTypeValue, TypeValue testTypeValue)
            {
                TestTargetTypeValue = testTargetTypeValue;
                TestTypeValue = testTypeValue;
            }
        }

        public static TestEnum MakeTestEnum(TypeValue testTargetTypeValue, string elemName) => new TestEnum(testTargetTypeValue, elemName);
        public static TestClass MakeTestClass(TypeValue testTargetTypeValue, TypeValue testTypeValue) => new TestClass(testTargetTypeValue, testTypeValue);
    }

    public class TaskStmtInfo : SyntaxNodeInfo
    {
        public CaptureInfo CaptureInfo { get; }
        public int LocalVarCount { get; }

        public TaskStmtInfo(CaptureInfo captureInfo, int localVarCount)
        {
            CaptureInfo = captureInfo;
            LocalVarCount = localVarCount;
        }

    }

    public class AsyncStmtInfo : SyntaxNodeInfo
    {
        public CaptureInfo CaptureInfo { get; }
        public int LocalVarCount { get; }

        public AsyncStmtInfo(CaptureInfo captureInfo, int localVarCount)
        {
            CaptureInfo = captureInfo;
            LocalVarCount = localVarCount;
        }
    }

    public class ForStmtInfo : SyntaxNodeInfo
    {
        public TypeValue? ContTypeValue { get; }
        public ForStmtInfo(TypeValue? contTypeValue)
        {
            ContTypeValue = contTypeValue;
        }
    }

    public class ExpForStmtInitializerInfo : SyntaxNodeInfo
    {
        public TypeValue ExpTypeValue { get; }
        public ExpForStmtInitializerInfo(TypeValue expTypeValue)
        {
            ExpTypeValue = expTypeValue;
        }
    }

    public class ForeachStmtInfo : SyntaxNodeInfo
    {
        public TypeValue ObjTypeValue { get; }
        public TypeValue EnumeratorTypeValue { get; }

        public TypeValue ElemTypeValue { get; }
        public int ElemLocalIndex { get; }
        public FuncValue GetEnumeratorValue { get; }
        public FuncValue MoveNextValue { get; }
        public FuncValue GetCurrentValue { get; }        

        public ForeachStmtInfo(TypeValue objTypeValue, TypeValue enumeratorTypeValue, TypeValue elemTypeValue, int elemLocalIndex, FuncValue getEnumeratorValue, FuncValue moveNextValue, FuncValue getCurrentValue)
        {
            ObjTypeValue = objTypeValue;
            EnumeratorTypeValue = enumeratorTypeValue;

            ElemTypeValue = elemTypeValue;
            ElemLocalIndex = elemLocalIndex;
            GetEnumeratorValue = getEnumeratorValue;
            MoveNextValue = moveNextValue;
            GetCurrentValue = getCurrentValue;
        }
    }

    public class ScriptInfo : SyntaxNodeInfo
    {
        public int LocalVarCount { get; }
        public ScriptInfo(int localVarCount)
        {
            LocalVarCount = localVarCount;
        }
    }

    public class IndexerExpInfo : SyntaxNodeInfo
    {
        public FuncValue FuncValue { get; }
        public TypeValue ObjectTypeValue { get; }
        public TypeValue IndexTypeValue { get; }

        public IndexerExpInfo(FuncValue funcValue, TypeValue objectTypeValue, TypeValue indexTypeValue)
        {
            FuncValue = funcValue;
            ObjectTypeValue = objectTypeValue;
            IndexTypeValue = indexTypeValue;
        }
    }    

    public class ExpStringExpElementInfo : SyntaxNodeInfo
    {
        public TypeValue ExpTypeValue { get; }
        public ExpStringExpElementInfo(TypeValue expTypeValue) 
        { 
            ExpTypeValue = expTypeValue; 
        }
    }
}
