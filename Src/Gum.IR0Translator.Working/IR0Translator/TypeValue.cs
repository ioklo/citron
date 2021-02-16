
using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    // 모두 immutable
    abstract partial class TypeValue : ItemValue
    {
        public virtual TypeValue? GetBaseType() { return null; }

        public virtual ItemResult GetMember(M.Name memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType) { return null; }
        public virtual TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) { return null; }        
        internal abstract TypeValue Apply(TypeEnv typeEnv);

        public abstract IR0.Type GetRType();
    }

    // "var"
    class VarTypeValue : TypeValue
    {
        public static VarTypeValue Instance { get; } = new VarTypeValue();
        private VarTypeValue() { }
        internal override TypeValue Apply(TypeEnv typeEnv) { return this; }
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

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            var typeValue = typeEnv.GetValue(Depth, Index);
            if (typeValue != null)
                return typeValue;

            return this;
        }
    }

    class ClassTypeValue : NormalTypeValue
    {
        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }
    }

    class StructTypeValue : NormalTypeValue
    {
        TypeValueFactory typeValueFactory;

        // global일 경우
        M.ModuleName? moduleName;
        M.NamespacePath? namespacePath;

        // member일 경우
        TypeValue? outer;        

        M.StructInfo structInfo;
        ImmutableArray<TypeValue> typeArgs;

        public StructTypeValue(TypeValueFactory typeValueFactory, M.ModuleName moduleName, M.NamespacePath namespacePath, M.StructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
            : this(typeValueFactory, moduleName, namespacePath, null, structInfo, typeArgs)
        {
        }
        
        public StructTypeValue(TypeValueFactory typeValueFactory, TypeValue outer, M.StructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
            : this(typeValueFactory, null, null, outer, structInfo, typeArgs)
        {
        }

        StructTypeValue(TypeValueFactory factory, M.ModuleName? moduleName, M.NamespacePath? namespacePath, TypeValue? outer, M.StructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.typeValueFactory = factory;
            this.moduleName = moduleName;
            this.namespacePath = namespacePath;
            this.outer = outer;
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

    }

    // (struct, class, enum) (external/internal) (global/member) type
    abstract partial class NormalTypeValue : TypeValue
    {   
    }

    // "void"
    class VoidTypeValue : TypeValue
    {
        public static VoidTypeValue Instance { get; } = new VoidTypeValue();
        private VoidTypeValue() { }

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            return this;
        }
    }

    // ArgTypeValues => RetValueTypes
    class LambdaTypeValue : TypeValue
    {
        public int LambdaId { get; }
        public TypeValue Return { get; }
        public ImmutableArray<TypeValue> Params { get; }

        public LambdaTypeValue(int lambdaId, TypeValue ret, ImmutableArray<TypeValue> parameters)
        {
            LambdaId = lambdaId;
            Return = ret;
            Params = parameters;
        }

        // lambdatypevalue를 replace할 수 있는가
        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            throw new InvalidOperationException();
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

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }
    }
}
