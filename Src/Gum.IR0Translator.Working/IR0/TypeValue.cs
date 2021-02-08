
using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;

namespace Gum.IR0
{
    // 모두 immutable
    abstract partial class TypeValue : ItemValue
    {
        public virtual ItemResult? GetMember(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType) { return null; }
        public virtual TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) { return null; }
        public abstract TypeValue Apply(ITypeEnv typeEnv);
    }

    // "var"
    class VarTypeValue : TypeValue
    {
        public static VarTypeValue Instance { get; } = new VarTypeValue();
        private VarTypeValue() { }
        public override TypeValue Apply(ITypeEnv typeEnv) { return this; }
    }

    // T: depth는 지역적이므로, 주어진 컨텍스트 안에서만 의미가 있다
    class TypeVarTypeValue : TypeValue
    {
        public int Depth { get; }
        public int Index { get; }
        public string Name { get; }

        public TypeVarTypeValue(int depth, int index, string name)
        {
            Depth = depth;
            Name = name;
        }

        public override TypeValue Apply(ITypeEnv typeEnv)
        {
            var typeValue = typeEnv.GetValue(Depth, Index);
            if (typeValue != null)
                return typeValue;

            return this;
        }
    }

    class ClassTypeValue : NormalTypeValue
    {

    }

    class StructTypeValue : NormalTypeValue
    {
        TypeValueFactory typeValueFactory;

        // global일 경우
        M.ModuleName? moduleName;
        M.NamespacePath? namespacePath;

        // member일 경우
        NormalTypeValue? outer;
        ITypeEnv typeEnv;

        M.StructInfo structInfo;
        ImmutableArray<TypeValue> typeArgs;

        public StructTypeValue(TypeValueFactory typeValueFactory, M.ModuleName moduleName, M.NamespacePath namespacePath, ITypeEnv typeEnv, M.StructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
            : base(typeValueFactory, moduleName, namespacePath, typeEnv)
        {
            this.structInfo = structInfo;
            this.typeArgs = typeArgs;
        }
        
        public StructTypeValue(TypeValueFactory typeValueFactory, TypeValue outer, M.StructInfo structInfo, ImmutableArray<TypeValue> typeArgs, ITypeEnv typeEnv)
            : base(typeValueFactory, outer, typeEnv)
        {
            this.structInfo = structInfo;
            this.typeArgs = typeArgs;
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
                return MultipleCandidatesItemResult.Instance;

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
                return MultipleCandidatesItemResult.Instance;

            if (results.Count == 0)
                return NotFoundItemResult.Instance;

            return results[0];
        }

        ItemResult GetMemberVar(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType)
        {
            // 타입 아규먼트가 있다면 이것은 변수가 아니다 (에러 아님)
            if (typeArgs.Length != 0) return NotFoundItemResult.Instance;

            var results = new List<ValueItemResult>();
            foreach (var memberVar in structInfo.MemberVars)
                if (memberVar.Name.Equals(memberName))
                {
                    var memberVarValue = typeValueFactory.MakeMemberVarValue(this, memberVar);
                    results.Add(new ValueItemResult(memberVarValue));
                }

            if (1 < results.Count)
                return MultipleCandidatesItemResult.Instance;

            if (results.Count == 0)
                return NotFoundItemResult.Instance;

            return results[0];
        }        

        public override ItemResult GetMember(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
        {
            // TODO: caching
            var results = new List<ValueItemResult>();

            // multiple, notfound, found
            var typeResult = GetMemberType(memberName, typeArgs, hintTypeValue);
            if (typeResult is MultipleCandidatesItemResult) return typeResult;
            if (typeResult is ValueItemResult typeMemberResult) results.Add(typeMemberResult);

            // multiple, notfound, found
            var funcResult = GetMemberFunc(memberName, typeArgs, hintTypeValue);
            if (funcResult is MultipleCandidatesItemResult) return funcResult;
            if (funcResult is ValueItemResult funcMemberResult) results.Add(funcMemberResult);

            // multiple notfound, found
            var varResult = GetMemberVar(memberName, typeArgs, hintTypeValue);
            if (varResult is MultipleCandidatesItemResult) return varResult;
            if (varResult is ValueItemResult varMemberResult) results.Add(varMemberResult);            

            if (1 < results.Count)
                return MultipleCandidatesItemResult.Instance;

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

        public override TypeValue Apply(ITypeEnv typeEnv)
        {
            // [X<00T>.Y<10U>.Z<20V>].Apply([00 => int, 10 => short, 20 => string])
            var typeInfo = GetTypeInfo();
            var typeArgs = GetTypeArgs();

            if (outer != null)
            {
                var appliedOuter = outer.Apply(typeEnv);
                var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
                return typeValueFactory.MakeStructType(appliedOuter, typeInfo, appliedTypeArgs);
            }
            else
            {
                Debug.Assert(moduleName != null && namespacePath != null);

                var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
                return typeValueFactory.MakeGlobalType(moduleName.Value, namespacePath.Value, typeInfo, appliedTypeArgs);
            }
        }

    }

    // (struct, class, enum) (external/internal) (global/member) type
    abstract partial class NormalTypeValue : TypeValue
    {
        
        // global
        protected NormalTypeValue(TypeValueFactory typeValueFactory, M.ModuleName moduleName, M.NamespacePath namespacePath, ITypeEnv typeEnv)
            : this(typeValueFactory, moduleName, namespacePath, null, typeEnv)
        {
        }

        // member
        protected NormalTypeValue(TypeValueFactory typeValueFactory, NormalTypeValue outer, ITypeEnv typeEnv)
            : this(typeValueFactory, null, null, outer, typeEnv)
        {
        }

        NormalTypeValue(TypeValueFactory typeValueFactory, M.ModuleName? moduleName, M.NamespacePath? namespacePath, NormalTypeValue? outer, ITypeEnv typeEnv)
        {
            this.typeValueFactory = typeValueFactory;
            this.moduleName = moduleName;
            this.namespacePath = namespacePath;
            this.outer = outer;
            this.typeEnv = typeEnv;
        }

        public ITypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        protected abstract M.TypeInfo GetTypeInfo();
        protected abstract ImmutableArray<TypeValue> GetTypeArgs();
    }

    // "void"
    class VoidTypeValue : TypeValue
    {
        public static VoidTypeValue Instance { get; } = new VoidTypeValue();
        private VoidTypeValue() { }
    }

    // ArgTypeValues => RetValueTypes
    class FuncTypeValue : TypeValue
    {
        public TypeValue Return { get; }
        public ImmutableArray<TypeValue> Params { get; }

        public FuncTypeValue(TypeValue ret, IEnumerable<TypeValue> parameters)
        {
            Return = ret;
            Params = parameters.ToImmutableArray();
        }
    }

    // S.First, S.Second(int i, short s)
    class EnumElemTypeValue : TypeValue
    {
        public NormalTypeValue EnumTypeValue { get; }
        public string Name { get; }

        public EnumElemTypeValue(NormalTypeValue enumTypeValue, string name)
        {
            EnumTypeValue = enumTypeValue;
            Name = name;
        }
    }
}
