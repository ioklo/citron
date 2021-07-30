using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using static Gum.Infra.Misc;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;

namespace Gum.IR0Translator
{
    // 모두 immutable
    abstract partial class TypeValue : ItemValue
    {   
        public virtual ItemQueryResult GetMember(M.Name memberName, int typeParamCount) { return ItemQueryResult.NotFound.Instance; }
        public virtual TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) { return null; }
        public abstract TypeValue Apply_TypeValue(TypeEnv typeEnv);        

        public sealed override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            return Apply_TypeValue(typeEnv);
        }

        public abstract R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member);
    }

    // "var"
    class VarTypeValue : TypeValue
    {
        public static readonly VarTypeValue Instance = new VarTypeValue();
        private VarTypeValue() { }

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv) { return this; }

        // var는 translation패스에서 추론되기 때문에 IR0에 없다
        public override R.Path GetRPath() { throw new InvalidOperationException();  }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();
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

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new NotImplementedException();        
    }

    [ImplementIEquatable]
    partial class ClassTypeValue : NormalTypeValue
    {
        public ClassTypeValue? GetBaseType() { throw new NotImplementedException(); }

        // except itself
        public bool IsBaseOf(ClassTypeValue derivedType)
        {
            ClassTypeValue? curBaseType = derivedType.GetBaseType();

            while(curBaseType != null)
            {
                if (Equals(curBaseType)) return true;
                curBaseType = curBaseType.GetBaseType();
            }

            return false;
        }

        public override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Path.Nested GetRPath_Nested() => throw new NotImplementedException();

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new NotImplementedException();
    }

    [AutoConstructor, ImplementIEquatable]
    partial class EnumTypeValue : NormalTypeValue
    {
        ItemValueFactory itemValueFactory;
        ItemValueOuter outer;
        IModuleEnumInfo enumInfo;
        ImmutableArray<TypeValue> typeArgs;        

        public override R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(enumInfo.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return outer.GetRPath(rname, new R.ParamHash(enumInfo.GetTypeParams().Length, default), rtypeArgs);
        }

        public EnumTypeValue Apply_EnumTypeValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply_TypeValue(typeEnv));
            return itemValueFactory.MakeEnumTypeValue(appliedOuter, enumInfo, appliedTypeArgs);
        }

        public sealed override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            return Apply_EnumTypeValue(typeEnv);
        }

        //
        public override ItemQueryResult GetMember(M.Name memberName, int typeParamCount) 
        {
            // shortcut
            if (typeParamCount != 0)
                return ItemQueryResult.NotFound.Instance;

            var elemInfo = enumInfo.GetElem(memberName);
            if (elemInfo == null) return ItemQueryResult.NotFound.Instance;

            return new ItemQueryResult.EnumElem(this, elemInfo);
        }

        public EnumElemTypeValue? GetElement(string name)
        {
            var elemInfo = enumInfo.GetElem(name);
            if (elemInfo == null) return null;

            return itemValueFactory.MakeEnumElemTypeValue(this, elemInfo);
        }

        public override TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) 
        {
            // shortcut
            if (typeArgs.Length != 0)
                return null;

            var elemInfo = enumInfo.GetElem(memberName);
            if (elemInfo == null) return null;

            return itemValueFactory.MakeEnumElemTypeValue(this, elemInfo);
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            if (outer != null)
                outer.FillTypeEnv(builder);

            for (int i = 0; i < enumInfo.GetTypeParams().Length; i++)
                builder.Add(typeArgs[i]);
        }
    }

    // S.First, S.Second(int i, short s)
    [AutoConstructor]
    partial class EnumElemTypeValue : NormalTypeValue
    {
        RItemFactory ritemFactory;
        ItemValueFactory itemValueFactory;
        public EnumTypeValue Outer { get; }
        IModuleEnumElemInfo elemInfo;        
        
        public bool IsStandalone()
        {
            return elemInfo.IsStandalone();
        }

        public override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            var appliedOuter = Outer.Apply_EnumTypeValue(typeEnv);
            return itemValueFactory.MakeEnumElemTypeValue(appliedOuter, elemInfo);
        }

        public override R.Path.Nested GetRPath_Nested()
        {
            var router = Outer.GetRPath_Nested();
            var rname = RItemFactory.MakeName(elemInfo.GetName());
            Debug.Assert(router != null);

            return new R.Path.Nested(router, rname, R.ParamHash.None, default);
        }

        public ImmutableArray<ParamInfo> GetConstructorParamTypes()
        {
            var fieldInfos = elemInfo.GetFieldInfos();

            var builder = ImmutableArray.CreateBuilder<ParamInfo>(fieldInfos.Length);
            foreach(var field in fieldInfos)
            {
                var fieldType = itemValueFactory.MakeTypeValueByMType(field.GetDeclType());
                var appliedFieldType = fieldType.Apply_TypeValue(Outer.MakeTypeEnv());

                builder.Add(new ParamInfo(R.ParamKind.Normal, appliedFieldType)); // TODO: EnumElemFields에 ref를 지원할지
            }

            return builder.MoveToImmutable();
        }

        public override ItemQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            foreach (var field in elemInfo.GetFieldInfos())
            {
                if (field.GetName().Equals(memberName))
                {
                    if (typeParamCount != 0)
                        return ItemQueryResult.Error.VarWithTypeArg.Instance;

                    return new ItemQueryResult.MemberVar(this, field);
                }
            }

            return ItemQueryResult.NotFound.Instance;
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => new R.EnumElemMemberLoc(instance, member);

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            Outer.FillTypeEnv(builder);
        }
    }

    [ImplementIEquatable]
    partial class StructTypeValue : NormalTypeValue
    {
        ItemValueFactory itemValueFactory;
        RItemFactory ritemFactory;

        ItemValueOuter outer;

        IModuleStructInfo structInfo;
        ImmutableArray<TypeValue> typeArgs;

        internal StructTypeValue(ItemValueFactory factory, RItemFactory ritemFactory, ItemValueOuter outer, IModuleStructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
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

            foreach (var memberType in structInfo.GetMemberTypes())
            {
                // 이름이 같고, 타입 파라미터 개수가 같다면
                if (memberType.GetName().Equals(memberName) && memberType.GetTypeParams().Length == typeParamCount)
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
            var funcsBuilder = ImmutableArray.CreateBuilder<IModuleFuncInfo>();

            bool bHaveInstance = false;
            bool bHaveStatic = false;
            foreach (var memberFunc in structInfo.GetMemberFuncs())
            {
                if (memberFunc.GetName().Equals(memberName) &&
                    typeParamCount <= memberFunc.GetTypeParams().Length)
                {
                    bHaveInstance |= memberFunc.IsInstanceFunc();
                    bHaveStatic |= !memberFunc.IsInstanceFunc();                    

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

            foreach (var memberVar in structInfo.GetMemberVars())
                if (memberVar.GetName().Equals(memberName))
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

        public TypeValue? GetBaseType()
        {
            var baseType = structInfo.GetBaseType();
            if (baseType == null) return null;

            var typeEnv = MakeTypeEnv();
            var typeValue = itemValueFactory.MakeTypeValueByMType(baseType);
            return typeValue.Apply_TypeValue(typeEnv);
        }

        public override ItemQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            if (memberName.Equals(M.SpecialNames.Constructor))
                return new ItemQueryResult.Constructors(this, structInfo.GetConstructors());

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
            foreach (var memberType in structInfo.GetMemberTypes())
            {
                if (memberType.GetName().Equals(memberName) && memberType.GetTypeParams().Length == typeArgs.Length)
                    return itemValueFactory.MakeTypeValue(this, memberType, typeArgs);
            }

            return null;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder) 
        {
            if (outer != null)
                outer.FillTypeEnv(builder);            

            for (int i = 0; i < structInfo.GetTypeParams().Length; i++)
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
            var rname = RItemFactory.MakeName(structInfo.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return outer.GetRPath(rname, new R.ParamHash(structInfo.GetTypeParams().Length, default), rtypeArgs);
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            return new R.StructMemberLoc(instance, member);
        }

        public IModuleConstructorInfo? GetAutoConstructor()
        {
            return structInfo.GetAutoConstructor();
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

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();        
    }

    // ArgTypeValues => RetValueTypes
    class LambdaTypeValue : TypeValue, IEquatable<LambdaTypeValue>
    {
        RItemFactory ritemFactory;
        public R.Path.Nested Lambda { get; } // Type의 path가 아니라 Lambda의 path
        public TypeValue Return { get; }
        public ImmutableArray<ParamInfo> Params { get; }

        public LambdaTypeValue(RItemFactory ritemFactory, R.Path.Nested lambda, TypeValue ret, ImmutableArray<ParamInfo> parameters)
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
            return Lambda;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Lambda);
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();
    }

    // seq ref int F(ref int a, ref int b) { yield ref a; }
    [AutoConstructor]
    partial class SeqTypeValue : TypeValue
    {
        RItemFactory ritemFactory;
        R.Path.Nested seqFunc;
        public TypeValue YieldType { get; }

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            throw new NotImplementedException();
        }

        public override R.Path GetRPath()
        {
            return seqFunc;
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();
    }

    [AutoConstructor]
    partial class TupleTypeValue : TypeValue
    {
        RItemFactory ritemFactory;
        public ImmutableArray<(TypeValue Type, string? Name)> Elems { get; }

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {   
            throw new NotImplementedException();
        }

        public override R.Path GetRPath()
        {
            var builder = ImmutableArray.CreateBuilder<R.TupleTypeElem>(Elems.Length);
            foreach(var elem in Elems)
            {
                var rpath = elem.Type.GetRPath();
                if (elem.Name == null)
                    throw new NotImplementedException(); // unnamed tuple
                var name = elem.Name;

                builder.Add(new R.TupleTypeElem(rpath, name));
            }

            return ritemFactory.MakeTupleType(builder.MoveToImmutable());
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new NotImplementedException();
    }

    // 런타임 라이브러리로 구현할 리스트 타입
    [AutoConstructor]
    partial class RuntimeListTypeValue : TypeValue
    {
        public ItemValueFactory itemValueFactory;
        public TypeValue ElemType { get; }

        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            var appliedElemType = ElemType.Apply_TypeValue(typeEnv);
            return itemValueFactory.MakeListType(appliedElemType);
        }

        public override R.Path GetRPath()
        {
            var runtime = new R.Path.Root("System.Runtime");
            var runtimeSystem = new R.Path.Nested(runtime, "System", R.ParamHash.None, default);
            var runtimeSystemList = new R.Path.Nested(runtimeSystem, "List", new R.ParamHash(1, default), Arr(ElemType.GetRPath()));

            return runtimeSystemList;
        }
        
        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            throw new NotImplementedException();
        }

        public R.Path GetIterRPath()
        {
            var runtime = new R.Path.Root("System.Runtime");
            var runtimeSystem = new R.Path.Nested(runtime, "System", R.ParamHash.None, default);
            var runtimeSystemList = new R.Path.Nested(runtimeSystem, "List", new R.ParamHash(1, default), Arr(ElemType.GetRPath()));
            var runtimeSystemListIter = new R.Path.Nested(runtimeSystemList, new R.Name.Anonymous(new R.AnonymousId(0)), R.ParamHash.None, default);

            return runtimeSystemListIter;
        }
    }
}
