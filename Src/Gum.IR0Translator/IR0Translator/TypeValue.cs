﻿
using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;

namespace Gum.IR0Translator
{
    // 모두 immutable
    abstract partial class TypeValue : ItemValue
    {
        public virtual TypeValue? GetBaseType() { return null; }
        public virtual ItemResult GetMember(M.Name memberName, ImmutableArray<TypeValue> typeArgs, ResolveHint hint) { return NotFoundItemResult.Instance; }
        public virtual TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) { return null; }        
        internal abstract TypeValue Apply(TypeEnv typeEnv);
        public abstract IR0.Type GetRType();
    }

    // "var"
    class VarTypeValue : TypeValue
    {
        public static readonly VarTypeValue Instance = new VarTypeValue();
        private VarTypeValue() { }

        internal override TypeValue Apply(TypeEnv typeEnv) { return this; }

        // var는 translation패스에서 추론되기 때문에 IR0에 없다
        public override R.Type GetRType() { throw new InvalidOperationException();  }
    }

    // T: depth는 지역적이므로, 주어진 컨텍스트 안에서만 의미가 있다
    [AutoConstructor, ImplementIEquatable]
    partial class TypeVarTypeValue : TypeValue
    {
        R.ItemFactory ritemFactory;

        public int Depth { get; }
        public int Index { get; }

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            var typeValue = typeEnv.GetValue(Depth, Index);
            if (typeValue != null)
                return typeValue;

            return this;
        }
        public override R.Type GetRType() => ritemFactory.MakeTypeVar(Depth, Index);
    }

    [ImplementIEquatable]
    partial class ClassTypeValue : NormalTypeValue
    {
        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Type GetRType() => throw new NotImplementedException();
    }

    [ImplementIEquatable]
    partial class EnumTypeValue : NormalTypeValue
    {
        public override R.Type GetRType()
        {
            throw new NotImplementedException();
        }

        internal override TypeValue Apply(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }
    }

    [ImplementIEquatable]
    partial class StructTypeValue : NormalTypeValue
    {
        ItemValueFactory typeValueFactory;
        R.ItemFactory ritemFactory;

        // global일 경우
        M.ModuleName? moduleName;
        M.NamespacePath? namespacePath;

        // member일 경우
        TypeValue? outer;        

        M.StructInfo structInfo;
        ImmutableArray<TypeValue> typeArgs;

        internal StructTypeValue(ItemValueFactory factory, R.ItemFactory ritemFactory, M.ModuleName? moduleName, M.NamespacePath? namespacePath, TypeValue? outer, M.StructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.typeValueFactory = factory;
            this.ritemFactory = ritemFactory;
            this.moduleName = moduleName;
            this.namespacePath = namespacePath;
            this.outer = outer;
            this.structInfo = structInfo;
            this.typeArgs = typeArgs;
        }

        ItemResult GetMember_Type(M.Name memberName, ImmutableArray<TypeValue> typeArgs)
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

        ItemResult GetMember_Func(M.Name memberName, ImmutableArray<TypeValue> typeArgs, ResolveHint resolveHint)
        {
            var results = new List<ValueItemResult>();

            foreach (var memberFunc in structInfo.MemberFuncs)
            {
                // TODO: resolveHint로 파라미터 체크 
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
        
        ItemResult GetMember_Var(M.Name memberName, ImmutableArray<TypeValue> typeArgs)
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

        public override ItemResult GetMember(M.Name memberName, ImmutableArray<TypeValue> typeArgs, ResolveHint hint)
        {
            // TODO: caching
            var results = new List<ValueItemResult>();

            // error, notfound, found
            var typeResult = GetMember_Type(memberName, typeArgs);
            if (typeResult is ErrorItemResult) return typeResult;
            if (typeResult is ValueItemResult typeMemberResult) results.Add(typeMemberResult);

            // error, notfound, found
            var funcResult = GetMember_Func(memberName, typeArgs, hint);
            if (funcResult is ErrorItemResult) return funcResult;
            if (funcResult is ValueItemResult funcMemberResult) results.Add(funcMemberResult);

            // error, notfound, found
            var varResult = GetMember_Var(memberName, typeArgs);
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

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    // ArgTypeValues => RetValueTypes
    class LambdaTypeValue : TypeValue, IEquatable<LambdaTypeValue>
    {
        R.ItemFactory ritemFactory;
        public R.DeclId LambdaDeclId { get; }
        public TypeValue Return { get; }
        public ImmutableArray<TypeValue> Params { get; }

        public LambdaTypeValue(R.ItemFactory ritemFactory, R.DeclId lambdaDeclId, TypeValue ret, ImmutableArray<TypeValue> parameters)
        {
            this.ritemFactory = ritemFactory;
            LambdaDeclId = lambdaDeclId;
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
            return LambdaDeclId.Equals(other.LambdaDeclId);
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

        public override int GetHashCode()
        {
            return HashCode.Combine(LambdaId);
        }
    }

    // S.First, S.Second(int i, short s)
    class EnumElemTypeValue : TypeValue
    {
        R.ItemFactory ritemFactory;
        public NormalTypeValue EnumTypeValue { get; }
        public string Name { get; }

        public EnumElemTypeValue(R.ItemFactory ritemFactory, NormalTypeValue enumTypeValue, string name)
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
