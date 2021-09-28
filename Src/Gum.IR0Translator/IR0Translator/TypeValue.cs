using System;
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

        public override int GetTotalTypeParamCount()
            => throw new InvalidOperationException();

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

        public override int GetTotalTypeParamCount()
        {
            throw new InvalidOperationException();
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new NotImplementedException();        
    }

    class NullableTypeValue : TypeValue
    {
        ItemValueFactory factory;
        TypeValue innerTypeValue;

        public NullableTypeValue(ItemValueFactory factory, TypeValue innerTypeValue)
        {
            this.factory = factory;
            this.innerTypeValue = innerTypeValue;
        }

        public R.Path GetInnerTypeRPath()
        {
            return innerTypeValue.GetRPath();
        }

        // T? => int?
        public override TypeValue Apply_TypeValue(TypeEnv typeEnv)
        {
            var applied = innerTypeValue.Apply_TypeValue(typeEnv);
            return factory.MakeNullableTypeValue(applied);
        }

        // 
        public override R.Path GetRPath()
        {
            var innerPath = innerTypeValue.GetRPath();
            return new R.Path.NullableType(innerPath);
        }

        public override int GetTotalTypeParamCount()
        {
            return 0;
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            throw new InvalidOperationException();
        }
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

            return new R.Path.Nested(outer.GetRPath(), rname, new R.ParamHash(enumInfo.GetTypeParams().Length, default), rtypeArgs);
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
            var elemInfo = enumInfo.GetElem(new M.Name.Normal(name));
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

        public override int GetTotalTypeParamCount()
        {
            return outer.GetTotalTypeParamCount() + enumInfo.GetTypeParams().Length;
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

        public override int GetTotalTypeParamCount()
        {
            return Outer.GetTotalTypeParamCount(); // Elem자체는 typeParams을 가질 수 없다
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

        public override int GetTotalTypeParamCount()
        {
            return 0;
        }
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

        public override int GetTotalTypeParamCount()
        {
            throw new NotImplementedException();
        }
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

        public override int GetTotalTypeParamCount()
        {
            throw new NotImplementedException();
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

                builder.Add(new R.TupleTypeElem(rpath, new R.Name.Normal(name)));
            }

            return ritemFactory.MakeTupleType(builder.MoveToImmutable());
        }

        public override int GetTotalTypeParamCount()
        {
            return 0;
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
            var runtimeSystem = new R.Path.Nested(runtime, new R.Name.Normal("System"), R.ParamHash.None, default);
            var runtimeSystemList = new R.Path.Nested(runtimeSystem, new R.Name.Normal("List"), new R.ParamHash(1, default), Arr(ElemType.GetRPath()));

            return runtimeSystemList;
        }
        
        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            throw new NotImplementedException();
        }

        public R.Path GetIterRPath()
        {
            var runtime = new R.Path.Root("System.Runtime");
            var runtimeSystem = new R.Path.Nested(runtime, new R.Name.Normal("System"), R.ParamHash.None, default);
            var runtimeSystemList = new R.Path.Nested(runtimeSystem, new R.Name.Normal("List"), new R.ParamHash(1, default), Arr(ElemType.GetRPath()));
            var runtimeSystemListIter = new R.Path.Nested(runtimeSystemList, new R.Name.Anonymous(0), R.ParamHash.None, default);

            return runtimeSystemListIter;
        }

        public override int GetTotalTypeParamCount()
        {
            return 1;
        }
    }
}
