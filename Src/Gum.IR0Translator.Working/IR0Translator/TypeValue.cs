
using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    // 모두 immutable
    abstract partial class TypeValue : ItemValue
    {
        public virtual TypeValue? GetBaseType() { return null; }
        public virtual ItemResult GetMember(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType) { return NotFoundItemResult.Instance; }
        public virtual TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) { return null; }        
        internal abstract TypeValue Apply(TypeEnv typeEnv);
        public abstract IR0.Type GetRType();
    }

    // "var"
    class VarTypeValue : TypeValue
    {
        public static readonly VarTypeValue Instance = new VarTypeValue();
        private VarTypeValue() { }
        public override bool Equals(object? obj)
        {
            return obj == Instance;
        }

        internal override TypeValue Apply(TypeEnv typeEnv) { return this; }

        // var는 translation패스에서 추론되기 때문에 IR0에 없다
        public override R.Type GetRType() { throw new InvalidOperationException();  }
    }

    // T: depth는 지역적이므로, 주어진 컨텍스트 안에서만 의미가 있다
    class TypeVarTypeValue : TypeValue
    {
        IR0ItemFactory ritemFactory;

        public int Depth { get; }
        public int Index { get; }

        public TypeVarTypeValue(IR0ItemFactory ritemFactory, int depth, int index)
        {
            this.ritemFactory = ritemFactory;
            Depth = depth;
            Index = index;
        }

        public override bool Equals(object? obj)
        {
            return obj is TypeVarTypeValue typeVar && Equals(typeVar);
        }

        public bool Equals(TypeVarTypeValue other)
        {
            return Depth == other.Depth && Index == other.Index;
        }

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            var typeValue = typeEnv.GetValue(Depth, Index);
            if (typeValue != null)
                return typeValue;

            return this;
        }
        public override R.Type GetRType() => ritemFactory.MakeTypeVar(Depth, Index);
    }

    class ClassTypeValue : NormalTypeValue
    {
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Type GetRType() => throw new NotImplementedException();
    }

    class StructTypeValue : NormalTypeValue
    {
        ItemValueFactory typeValueFactory;
        IR0ItemFactory ritemFactory;

        // global일 경우
        M.ModuleName? moduleName;
        M.NamespacePath? namespacePath;

        // member일 경우
        TypeValue? outer;        

        M.StructInfo structInfo;
        ImmutableArray<TypeValue> typeArgs;

        internal StructTypeValue(ItemValueFactory factory, IR0ItemFactory ritemFactory, M.ModuleName? moduleName, M.NamespacePath? namespacePath, TypeValue? outer, M.StructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.typeValueFactory = factory;
            this.ritemFactory = ritemFactory;
            this.moduleName = moduleName;
            this.namespacePath = namespacePath;
            this.outer = outer;
            this.structInfo = structInfo;
            this.typeArgs = typeArgs;
        }

        public override bool Equals(object? obj)
        {
            return obj is StructTypeValue structValue && Equals(structValue);
        }

        public bool Equals(StructTypeValue other)
        {
            return moduleName.Equals(other.moduleName) &&
                namespacePath.Equals(other.namespacePath) &&
                EqualityComparer<TypeValue>.Default.Equals(outer, other.outer) && // outer null처리를 위해 EqualityComparer
                structInfo.Equals(other.structInfo) &&
                typeArgs.SequenceEqual(other.typeArgs);
        }

        ItemResult GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType)
        {
            var results = new List<ValueItemResult>();

            foreach (var memberType in structInfo.MemberTypes)
            {
                // 이름이 같고, 타입 파라미터 개수가 같다면
                if (memberType.Name.Equals(memberName) && memberType.TypeParams.Length == typeArgs.Length)
                {
                    var typeValue = typeValueFactory.MakeMemberType(this, memberType, typeArgs);
                    results.Add(new ValueItemResult(typeValue));
                }
            }

            if (1 < results.Count)
                return MultipleCandidatesErrorItemResult.Instance;

            if (results.Count == 0)
                return NotFoundItemResult.Instance;

            return results[0];
        }

        ItemResult GetMemberFunc(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType)
        {
            var results = new List<ValueItemResult>();

            foreach (var memberFunc in structInfo.MemberFuncs)
            {
                // TODO: 1. 파라미터 체크 
                if (memberFunc.Name.Equals(memberName) && 
                    memberFunc.TypeParams.Length == typeArgs.Length) // TODO: TypeParam inference, 같지 않아도 된다
                {
                    var funcValue = typeValueFactory.MakeMemberFunc(this, memberFunc, typeArgs);
                    results.Add(new ValueItemResult(funcValue));
                }
            }

            if (1 < results.Count)
                return MultipleCandidatesErrorItemResult.Instance;

            if (results.Count == 0)
                return NotFoundItemResult.Instance;

            return results[0];
        }
        
        ItemResult GetMemberVar(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType)
        {
            var results = new List<ValueItemResult>();
            foreach (var memberVar in structInfo.MemberVars)
                if (memberVar.Name.Equals(memberName))
                {
                    var memberVarValue = typeValueFactory.MakeMemberVarValue(this, memberVar);
                    results.Add(new ValueItemResult(memberVarValue));
                }

            if (1 < results.Count)
                return MultipleCandidatesErrorItemResult.Instance;

            if (results.Count == 0)
                return NotFoundItemResult.Instance;

            // 변수를 찾았는데 타입 아규먼트가 있다면 에러
            if (typeArgs.Length != 0) 
                return VarWithTypeArgErrorItemResult.Instance;

            return results[0];
        }

        public override TypeValue? GetBaseType()
        {
            if (structInfo.BaseType == null) return null;

            var typeEnv = MakeTypeEnv();
            var typeValue = typeValueFactory.MakeTypeValue(structInfo.BaseType);
            return typeValue.Apply(typeEnv);
        }

        public override ItemResult GetMember(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
        {
            // TODO: caching
            var results = new List<ValueItemResult>();

            // error, notfound, found
            var typeResult = GetMemberType(memberName, typeArgs, hintTypeValue);
            if (typeResult is ErrorItemResult) return typeResult;
            if (typeResult is ValueItemResult typeMemberResult) results.Add(typeMemberResult);

            // error, notfound, found
            var funcResult = GetMemberFunc(memberName, typeArgs, hintTypeValue);
            if (funcResult is ErrorItemResult) return funcResult;
            if (funcResult is ValueItemResult funcMemberResult) results.Add(funcMemberResult);

            // error, notfound, found
            var varResult = GetMemberVar(memberName, typeArgs, hintTypeValue);
            if (varResult is ErrorItemResult) return varResult;
            if (varResult is ValueItemResult varMemberResult) results.Add(varMemberResult);            

            if (1 < results.Count)
                return MultipleCandidatesErrorItemResult.Instance;

            if (results.Count == 0)
                return NotFoundItemResult.Instance;

            return results[0];
        }

        public override TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs)
        {
            // TODO: caching
            foreach (var memberType in structInfo.MemberTypes)
            {
                if (memberType.Name.Equals(memberName) && memberType.TypeParams.Length == typeArgs.Length)
                    return typeValueFactory.MakeMemberType(this, memberType, typeArgs);
            }

            return null;
        }

        internal override int FillTypeEnv(TypeEnvBuilder builder) 
        {
            int depth;
            if (outer != null)
                depth = outer.FillTypeEnv(builder) + 1;
            else
                depth = 0;

            for (int i = 0; i < structInfo.TypeParams.Length; i++)
                builder.Add(depth, i, typeArgs[i]);

            return depth;
        }

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            // [X<00T>.Y<10U>.Z<20V>].Apply([00 => int, 10 => short, 20 => string])            

            if (outer != null)
            {
                var appliedOuter = outer.Apply(typeEnv);
                var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
                return typeValueFactory.MakeMemberType(appliedOuter, structInfo, appliedTypeArgs);
            }
            else
            {
                Debug.Assert(moduleName != null && namespacePath != null);

                var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
                return typeValueFactory.MakeGlobalType(moduleName.Value, namespacePath.Value, structInfo, appliedTypeArgs);
            }
        }

        bool IsGlobal()
        {
            if (moduleName != null && namespacePath != null)
                return true;
            else if (outer != null)
                return false;

            throw new UnreachableCodeException();
        }

        ImmutableArray<R.Type> GetRTypes(ImmutableArray<TypeValue> typeValues)
        {
            return ImmutableArray.CreateRange(typeValues, typeValue => typeValue.GetRType());
        }

        public override R.Type GetRType()
        {
            if (IsGlobal())
            {
                Debug.Assert(moduleName != null && namespacePath != null);

                var rtypeArgs = GetRTypes(typeArgs);
                return ritemFactory.MakeGlobalType(moduleName.Value, namespacePath.Value, structInfo.Name, rtypeArgs);
            }
            else
            {
                Debug.Assert(outer != null);

                var outerRType = outer.GetRType();
                var rtypeArgs = GetRTypes(typeArgs);
                return ritemFactory.MakeMemberType(outerRType, structInfo.Name, rtypeArgs);
            }
        }
    }

    // (struct, class, enum) (external/internal) (global/member) type
    abstract partial class NormalTypeValue : TypeValue
    {   
    }

    // "void"
    class VoidTypeValue : TypeValue
    {
        public static readonly VoidTypeValue Instance = new VoidTypeValue();
        private VoidTypeValue() { }
        public override bool Equals(object? obj)
        {
            return obj == Instance;
        }

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            return this;
        }

        public override R.Type GetRType()
        {
            return R.Type.Void;
        }
    }

    // ArgTypeValues => RetValueTypes
    class LambdaTypeValue : TypeValue, IEquatable<LambdaTypeValue>
    {
        IR0ItemFactory ritemFactory;
        public int LambdaId { get; }
        public TypeValue Return { get; }
        public ImmutableArray<TypeValue> Params { get; }

        public LambdaTypeValue(IR0ItemFactory ritemFactory, int lambdaId, TypeValue ret, ImmutableArray<TypeValue> parameters)
        {
            this.ritemFactory = ritemFactory;
            LambdaId = lambdaId;
            Return = ret;
            Params = parameters;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as LambdaTypeValue);
        }

        public bool Equals(LambdaTypeValue? other)
        {
            if (other == null) return false;

            // 아이디만 비교한다. 같은 위치에서 다른 TypeContext에서 생성되는 람다는 id도 바뀌어야 한다
            return LambdaId == other.LambdaId;
        }

        // lambdatypevalue를 replace할 일이 있는가
        // void Func<T>()
        // {
        //     var l = (T t) => t; // { l => LambdaTypeValue({id}, T, [T]) }
        // }
        // 분석중에 Apply할 일은 없고, 실행중에 할 일은 있을 것 같다
        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            throw new InvalidOperationException();
        }

        public override R.Type GetRType()
        {
            var returnRType = Return.GetRType();
            var paramRTypes = ImmutableArray.CreateRange(Params, parameter => parameter.GetRType());

            return ritemFactory.MakeLambdaType(LambdaId, returnRType, paramRTypes);
        }
    }

    // S.First, S.Second(int i, short s)
    class EnumElemTypeValue : TypeValue
    {
        IR0ItemFactory ritemFactory;
        public NormalTypeValue EnumTypeValue { get; }
        public string Name { get; }

        public EnumElemTypeValue(IR0ItemFactory ritemFactory, NormalTypeValue enumTypeValue, string name)
        {
            this.ritemFactory = ritemFactory;
            EnumTypeValue = enumTypeValue;
            Name = name;
        }

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Type GetRType()
        {
            return ritemFactory.MakeEnumElemType();
        }
    }
}
