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
        public virtual ItemQueryResult GetMember(M.Name memberName, int typeParamCount) { return ItemQueryResult.NotFound.Instance; }
        public virtual TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) { return null; }        
        public abstract TypeValue Apply_TypeValue(TypeEnv typeEnv);        

        public sealed override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            return Apply_TypeValue(typeEnv);
        }
    }

    // "var"
    class VarTypeValue : TypeValue
    {
        public static readonly VarTypeValue Instance = new VarTypeValue();
        private VarTypeValue() { }

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv) { return this; }

        // var는 translation패스에서 추론되기 때문에 IR0에 없다
        public override R.Path GetRPath() { throw new InvalidOperationException();  }
    }

    // T: depth는 지역적이므로, 주어진 컨텍스트 안에서만 의미가 있다
    [AutoConstructor, ImplementIEquatable]
    partial class TypeVarTypeValue : TypeValue
    {
        RItemFactory ritemFactory;

        public int Index { get; }

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            var typeValue = typeEnv.GetValue(Index);
            if (typeValue != null)
                return typeValue;

            return this;
        }
        public override R.Path GetRPath() => new R.Path.TypeVarType(Index);
    }

    [ImplementIEquatable]
    partial class ClassTypeValue : NormalTypeValue
    {
        public override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Path.Nested GetRPath_Nested() => throw new NotImplementedException();
    }

    [ImplementIEquatable]
    partial class EnumTypeValue : NormalTypeValue
    {
        public override R.Path.Nested GetRPath_Nested()
        {
            throw new NotImplementedException();
        }

        public override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }
    }

    [ImplementIEquatable]
    partial class StructTypeValue : NormalTypeValue
    {
        ItemValueFactory itemValueFactory;
        RItemFactory ritemFactory;

        ItemValueOuter outer;

        M.StructInfo structInfo;
        ImmutableArray<TypeValue> typeArgs;

        internal StructTypeValue(ItemValueFactory factory, RItemFactory ritemFactory, ItemValueOuter outer, M.StructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.itemValueFactory = factory;
            this.ritemFactory = ritemFactory;
            this.outer = outer;
            this.structInfo = structInfo;
            this.typeArgs = typeArgs;
        }

        ItemQueryResult GetMember_Type(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<ItemQueryResult.Valid>();

            foreach (var memberType in structInfo.MemberTypes)
            {
                // 이름이 같고, 타입 파라미터 개수가 같다면
                if (memberType.Name.Equals(memberName) && memberType.TypeParams.Length == typeParamCount)
                {
                    candidates.Add(new ItemQueryResult.Type(new NestedItemValueOuter(this), memberType));
                }
            }

            var result = candidates.GetSingle();
            if (result != null)
                return result;

            if (candidates.HasMultiple)
                return ItemQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return ItemQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }
        
        ItemQueryResult GetMember_Func(M.Name memberName, int typeParamCount)
        {
            var funcsBuilder = ImmutableArray.CreateBuilder<M.FuncInfo>();

            bool bHaveInstance = false;
            bool bHaveStatic = false;
            foreach (var memberFunc in structInfo.MemberFuncs)
            {
                if (memberFunc.Name.Equals(memberName) &&
                    typeParamCount <= memberFunc.TypeParams.Length)
                {
                    bHaveInstance |= memberFunc.IsInstanceFunc;
                    bHaveStatic |= !memberFunc.IsInstanceFunc;                    

                    funcsBuilder.Add(memberFunc);
                }
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (funcsBuilder.Count == 0)
                return ItemQueryResult.NotFound.Instance;

            // 둘다 가지고 있으면 안된다
            if (bHaveInstance && bHaveStatic)
                return ItemQueryResult.Error.MultipleCandidates.Instance;

            Debug.Assert(!bHaveInstance && !bHaveStatic);

            return new ItemQueryResult.Funcs(new NestedItemValueOuter(this), funcsBuilder.ToImmutable(), bHaveInstance);
        }
        
        ItemQueryResult GetMember_Var(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<ItemQueryResult.MemberVar>();

            foreach (var memberVar in structInfo.MemberVars)
                if (memberVar.Name.Equals(memberName))
                {   
                    candidates.Add(new ItemQueryResult.MemberVar(this, memberVar));
                }

            var result = candidates.GetSingle();
            if (result != null)
            {
                // 변수를 찾았는데 타입 아규먼트가 있다면 에러
                if (typeParamCount != 0)
                    return ItemQueryResult.Error.VarWithTypeArg.Instance;

                return result;
            }

            if (candidates.HasMultiple)
                return ItemQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return ItemQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }

        public override TypeValue? GetBaseType()
        {
            if (structInfo.BaseType == null) return null;

            var typeEnv = MakeTypeEnv();
            var typeValue = itemValueFactory.MakeTypeValue(structInfo.BaseType);
            return typeValue.Apply_TypeValue(typeEnv);
        }

        public override ItemQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            // TODO: caching
            var results = new List<ItemQueryResult.Valid>();

            // error, notfound, found
            var typeResult = GetMember_Type(memberName, typeParamCount);
            if (typeResult is ItemQueryResult.Error) return typeResult;
            if (typeResult is ItemQueryResult.Valid typeMemberResult) results.Add(typeMemberResult);

            // error, notfound, found
            var funcResult = GetMember_Func(memberName, typeParamCount);
            if (funcResult is ItemQueryResult.Error) return funcResult;
            if (funcResult is ItemQueryResult.Valid funcMemberResult) results.Add(funcMemberResult);

            // error, notfound, found
            var varResult = GetMember_Var(memberName, typeParamCount);
            if (varResult is ItemQueryResult.Error) return varResult;
            if (varResult is ItemQueryResult.Valid varMemberResult) results.Add(varMemberResult);            

            if (1 < results.Count)
                return ItemQueryResult.Error.MultipleCandidates.Instance;

            if (results.Count == 0)
                return ItemQueryResult.NotFound.Instance;

            return results[0];
        }

        public override TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs)
        {
            // TODO: caching
            foreach (var memberType in structInfo.MemberTypes)
            {
                if (memberType.Name.Equals(memberName) && memberType.TypeParams.Length == typeArgs.Length)
                    return itemValueFactory.MakeTypeValue(this, memberType, typeArgs);
            }

            return null;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder) 
        {
            if (outer != null)
                outer.FillTypeEnv(builder);            

            for (int i = 0; i < structInfo.TypeParams.Length; i++)
                builder.Add(typeArgs[i]);
        }

        public override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            // [X<00T>.Y<10U>.Z<20V>].Apply([00 => int, 10 => short, 20 => string])            

            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply_TypeValue(typeEnv));
            return itemValueFactory.MakeTypeValue(appliedOuter, structInfo, appliedTypeArgs);
        }
        
        public override R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(structInfo.Name);
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return outer.GetRPath(rname, new R.ParamHash(structInfo.TypeParams.Length, default), rtypeArgs);
        }
    }

    // (struct, class, enum) (external/internal) (global/member) type
    abstract partial class NormalTypeValue : TypeValue
    {
        public abstract NormalTypeValue Apply_NormalTypeValue(TypeEnv env);
        public sealed override TypeValue Apply_TypeValue(TypeEnv env)
        {
            return Apply_NormalTypeValue(env);
        }

        public sealed override R.Path GetRPath()
        {
            return GetRPath_Nested();
        }

        public abstract R.Path.Nested GetRPath_Nested();        
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

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            return this;
        }

        public override R.Path GetRPath()
        {
            return R.Path.VoidType.Instance;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    // ArgTypeValues => RetValueTypes
    class LambdaTypeValue : TypeValue, IEquatable<LambdaTypeValue>
    {
        RItemFactory ritemFactory;
        public R.Path.Nested Lambda { get; } // Type의 path가 아니라 Lambda의 path
        public TypeValue Return { get; }
        public ImmutableArray<TypeValue> Params { get; }

        public LambdaTypeValue(RItemFactory ritemFactory, R.Path.Nested lambda, TypeValue ret, ImmutableArray<TypeValue> parameters)
        {
            this.ritemFactory = ritemFactory;
            this.Lambda = lambda;
            this.Return = ret;
            this.Params = parameters;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as LambdaTypeValue);
        }

        public bool Equals(LambdaTypeValue? other)
        {
            if (other == null) return false;

            // 아이디만 비교한다. 같은 위치에서 다른 TypeContext에서 생성되는 람다는 id도 바뀌어야 한다
            return Lambda.Equals(other.Lambda);
        }

        // lambdatypevalue를 replace할 일이 있는가
        // void Func<T>()
        // {
        //     var l = (T t) => t; // { l => LambdaTypeValue({id}, T, [T]) }
        // }
        // 분석중에 Apply할 일은 없고, 실행중에 할 일은 있을 것 같다
        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            throw new InvalidOperationException();
        }

        public override R.Path GetRPath()
        {
            return ritemFactory.MakeLambdaType(Lambda);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Lambda);
        }
    }

    // 
    [AutoConstructor]
    partial class SeqTypeValue : TypeValue
    {
        RItemFactory ritemFactory;
        R.Path.Nested seqFunc;
        TypeValue yieldType;

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Path GetRPath()
        {
            return new R.Path.AnonymousSeqType(seqFunc);
        }
    }

    // S.First, S.Second(int i, short s)
    class EnumElemTypeValue : TypeValue
    {
        RItemFactory ritemFactory;
        public NormalTypeValue EnumTypeValue { get; }
        public string Name { get; }

        public EnumElemTypeValue(RItemFactory ritemFactory, NormalTypeValue enumTypeValue, string name)
        {
            this.ritemFactory = ritemFactory;
            EnumTypeValue = enumTypeValue;
            Name = name;
        }

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Path GetRPath()
        {
            return ritemFactory.MakeEnumElemType();
        }
    }
}
