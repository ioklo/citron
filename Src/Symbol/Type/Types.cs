using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    public interface IType : ICyclicEqualityComparableClass<IType>, ISerializable
    {
        IType GetTypeArg(int index);
        IType Apply(TypeEnv typeEnv);
        TypeId GetTypeId();
        IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs); // 이름에 해당하는 멤버타입을 가져온다
        SymbolQueryResult? QueryMember(Name name, int explicitTypeArgCount);

        void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ITypeVisitor;
    }

    public abstract record class TypeImpl : IType
    {
        public virtual IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException(); // TypeArg를 불러올 일이 없어야 한다
        }
        public abstract IType Apply(TypeEnv typeEnv);
        public abstract TypeId GetTypeId();
        public abstract IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs);
        public abstract void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ITypeVisitor;

        // 내부의 값들이 다 같은지 확인
        public abstract bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context);
        public abstract void DoSerialize(ref SerializeContext context);
        public abstract SymbolQueryResult? QueryMember(Name name, int explicitTypeArgCount);
    }

    public abstract record class SymbolType : TypeImpl
    {
        protected abstract ITypeSymbol GetTypeSymbol();

        public sealed override IType GetTypeArg(int index) => GetTypeSymbol().GetTypeArg(index);
        public sealed override TypeId GetTypeId() => GetTypeSymbol().GetSymbolId();
        public sealed override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => GetTypeSymbol().GetMemberType(name, typeArgs);

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        {
            return other is SymbolType otherSymbolType && GetTypeSymbol().CyclicEquals(otherSymbolType.GetTypeSymbol(), ref context);
        }

        public sealed override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(Symbol), GetTypeSymbol());
        }

        public sealed override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
        {
            return GetTypeSymbol().QueryMember(name, typeArgCount);
        }
    }

    public record class EnumElemType(EnumElemSymbol Symbol) : SymbolType
    {
        protected override ITypeSymbol GetTypeSymbol() => Symbol;
        public override EnumElemType Apply(TypeEnv typeEnv) => new EnumElemType(Symbol.Apply(typeEnv));
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitEnumElem(this);
    }

    public record class EnumType(EnumSymbol Symbol) : SymbolType
    {
        protected override ITypeSymbol GetTypeSymbol() => Symbol;
        public override EnumType Apply(TypeEnv typeEnv) => new EnumType(Symbol.Apply(typeEnv));        
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitEnum(this);
    }

    public record class ClassType(ClassSymbol Symbol) : SymbolType, ICyclicEqualityComparableClass<ClassType>
    {
        protected override ITypeSymbol GetTypeSymbol() => Symbol;
        public override ClassType Apply(TypeEnv typeEnv) => new ClassType(Symbol.Apply(typeEnv));
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitClass(this);

        bool ICyclicEqualityComparableClass<ClassType>.CyclicEquals(ClassType other, ref CyclicEqualityCompareContext context)
            => base.CyclicEquals(other, ref context);
    }

    public record class InterfaceType(InterfaceSymbol Symbol) : SymbolType, ICyclicEqualityComparableClass<InterfaceType>
    {
        protected override ITypeSymbol GetTypeSymbol() => Symbol;
        public override InterfaceType Apply(TypeEnv typeEnv) => new InterfaceType(Symbol.Apply(typeEnv));
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitInterface(this);

        bool ICyclicEqualityComparableClass<InterfaceType>.CyclicEquals(InterfaceType other, ref CyclicEqualityCompareContext context)
            => base.CyclicEquals(other, ref context);
    }

    public record class StructType(StructSymbol Symbol) : SymbolType, ICyclicEqualityComparableClass<StructType>
    {
        protected override ITypeSymbol GetTypeSymbol() => Symbol;
        public override StructType Apply(TypeEnv typeEnv) => new StructType(Symbol.Apply(typeEnv));
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitStruct(this);

        bool ICyclicEqualityComparableClass<StructType>.CyclicEquals(StructType other, ref CyclicEqualityCompareContext context)
            => base.CyclicEquals(other, ref context);
    }

    public record class LambdaType(LambdaSymbol Symbol) : SymbolType, ICyclicEqualityComparableClass<LambdaType>
    {
        protected override ITypeSymbol GetTypeSymbol() => Symbol;
        public override LambdaType Apply(TypeEnv typeEnv) => new LambdaType(Symbol.Apply(typeEnv));
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitLambda(this);

        bool ICyclicEqualityComparableClass<LambdaType>.CyclicEquals(LambdaType other, ref CyclicEqualityCompareContext context)
            => base.CyclicEquals(other, ref context);
    }

    public record class NullableType(IType InnerType) : TypeImpl
    {   
        public override NullableType Apply(TypeEnv typeEnv) => new NullableType(InnerType.Apply(typeEnv));
        public override TypeId GetTypeId() => new NullableTypeId(InnerType.GetTypeId());
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;

        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitNullable(this);

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => other is NullableType otherType && CyclicEquals(otherType, ref context);

        bool CyclicEquals(NullableType other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(InnerType, other.InnerType))
                return false;

            return true;
        }

        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(InnerType), InnerType);
        }

        public override SymbolQueryResult? QueryMember(Name name, int explicitTypeArgCount)
        {
            return null;
        }
    }

    public record class TypeVarType(int Index, Name Name) : TypeImpl
    {   
        public override IType Apply(TypeEnv typeEnv) => typeEnv.GetValue(Index);
        public override TypeId GetTypeId() => new TypeVarTypeId(Index, Name);
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitTypeVar(this);

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => other is TypeVarType otherType && CyclicEquals(otherType, ref context);

        bool CyclicEquals(TypeVarType other, ref CyclicEqualityCompareContext context)
        {
            if (!Index.Equals(other.Index))
                return false;

            if (!Name.Equals(other.Name))
                return false;

            return true;
        }

        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeInt(nameof(Index), Index);
            context.SerializeRef(nameof(Name), Name);
        }

        public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
        {
            return null;
        }
    }

    public record class VoidType: TypeImpl
    {
        public override VoidType Apply(TypeEnv typeEnv) => new VoidType();
        public override TypeId GetTypeId() => new VoidTypeId();
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitVoid(this);

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => true;

        public override void DoSerialize(ref SerializeContext context)
        {
        }

        public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
        {
            return null;
        }
    }

    public record struct TupleMemberVar : ISerializable, ICyclicEqualityComparableStruct<TupleMemberVar>
    {
        IType declType;
        Name name;

        public TupleMemberVar(IType declType, Name name)
        {
            this.declType = declType;
            this.name = name;
        }

        public IType GetDeclType()
        {
            return declType;
        }

        public Name GetName()
        {
            return name;
        }

        public TupleMemberVar Apply(TypeEnv typeEnv)
        {
            return new TupleMemberVar(declType.Apply(typeEnv), name);
        }

        public void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(declType), declType);
            context.SerializeRef(nameof(name), name);
        }

        bool ICyclicEqualityComparableStruct<TupleMemberVar>.CyclicEquals(ref TupleMemberVar other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(this.GetDeclType(), other.declType))
                return false;

            if (!name.Equals(other.name))
                return false;

            return true;
        }
    }

    public record class TupleType : TypeImpl
    {
        ImmutableArray<TupleMemberVar> memberVars;

        public TupleType(ImmutableArray<TupleMemberVar> memberVars)
        {
            this.memberVars = memberVars;
        }

        public int GetMemberVarCount()
        {
            return memberVars.Length;
        }

        public TupleMemberVar GetMemberVar(int index)
        {
            return memberVars[index];
        }

        public override IType Apply(TypeEnv typeEnv)
        {
            var builder = ImmutableArray.CreateBuilder<TupleMemberVar>(memberVars.Length);
            foreach (var memberVar in memberVars)
                builder.Add(memberVar.Apply(typeEnv));
            return new TupleType(builder.MoveToImmutable());
        }

        public override TypeId GetTypeId()
        {
            var builder = ImmutableArray.CreateBuilder<TupleMemberVarId>(memberVars.Length);
            foreach (var memberVar in memberVars)
                builder.Add(new TupleMemberVarId(memberVar.GetDeclType().GetTypeId(), memberVar.GetName()));
            return new TupleTypeId(builder.MoveToImmutable());
        }
        
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitTuple(this);

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => other is TupleType otherType && CyclicEquals(otherType, ref context);

        bool CyclicEquals(TupleType other, ref CyclicEqualityCompareContext context)
        {
            return memberVars.CyclicEqualsStructItem(ref other.memberVars, ref context);
        }

        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeValueArray(nameof(memberVars), memberVars);
        }

        public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
        {
            foreach (var memberVar in memberVars)
                if (memberVar.GetName().Equals(name))
                    return new SymbolQueryResult.TupleMemberVar();

            return null;
        }
    }

    // c#의 delegate처럼 함수 매개변수의 프로퍼티를 다 보존한다
    public record class FuncType : TypeImpl
    {
        FuncReturn funcRet;
        ImmutableArray<FuncParameter> parameters;

        public FuncType(FuncReturn funcRet, ImmutableArray<FuncParameter> parameters)
        {
            this.funcRet = funcRet;
            this.parameters = parameters;
        }

        public override IType Apply(TypeEnv typeEnv)
        {
            var appliedReturn = funcRet.Apply(typeEnv);
            var appliedParametersBuilder = ImmutableArray.CreateBuilder<FuncParameter>(parameters.Length);
            foreach (var parameter in parameters)
            {
                var appliedParameter = parameter.Apply(typeEnv);
                appliedParametersBuilder.Add(appliedParameter);
            }

            return new FuncType(appliedReturn, appliedParametersBuilder.MoveToImmutable());
        }

        public override TypeId GetTypeId()
        {
            var builder = ImmutableArray.CreateBuilder<FuncParameterId>(parameters.Length);
            foreach (var parameter in parameters)
                builder.Add(parameter.GetId());

            return new FuncTypeId(funcRet.GetId(), builder.MoveToImmutable());
        }

        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitFunc(this);
        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => other is FuncType otherType && CyclicEquals(otherType, ref context);

        bool CyclicEquals(FuncType other, ref CyclicEqualityCompareContext context)
        {            
            if (!funcRet.CyclicEquals(ref other.funcRet, ref context))
                return false;

            if (!parameters.CyclicEqualsStructItem(ref parameters, ref context))
                return false;

            return true;
        }   

        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeValue(nameof(funcRet), funcRet);
            context.SerializeValueArray(nameof(parameters), parameters);
        }

        public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
        {
            return null;
        }

        public IType GetReturnType()
        {
            return funcRet.Type;
        }
    }

    public record class VarType : TypeImpl
    {
        public override IType Apply(TypeEnv typeEnv) => new VarType();
        public override TypeId GetTypeId() => new VarTypeId();
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitVar(this);
        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => true;

        public override void DoSerialize(ref SerializeContext context)
        {
        }

        public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
        {
            return null;
        }
    }

    public record class LocalPtrType(IType InnerType) : TypeImpl
    {
        public override IType Apply(TypeEnv typeEnv)
        {
            var appliedInnerType = InnerType.Apply(typeEnv);
            return new LocalPtrType(appliedInnerType);
        }

        public override TypeId GetTypeId() => new LocalPtrTypeId(InnerType.GetTypeId());
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitLocalPtr(this);

        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(InnerType), InnerType);
        }

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => other is LocalPtrType otherType && CyclicEquals(otherType, ref context);

        bool CyclicEquals(LocalPtrType other, ref CyclicEqualityCompareContext context)
        {
            if (!InnerType.CyclicEquals(other.InnerType, ref context))
                return false;

            return true;
        }

        public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
        {
            return null;
        }
    }

    public record class BoxPtrType(IType InnerType) : TypeImpl
    {
        public override IType Apply(TypeEnv typeEnv)
        {
            var appliedInnerType = InnerType.Apply(typeEnv);
            return new BoxPtrType(appliedInnerType);
        }

        public override TypeId GetTypeId() => new BoxPtrTypeId(InnerType.GetTypeId());
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitBoxPtr(this);

        public override void DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(InnerType), InnerType);
        }

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => other is BoxPtrType otherType && CyclicEquals(otherType, ref context);

        bool CyclicEquals(BoxPtrType other, ref CyclicEqualityCompareContext context)
        {
            if (!InnerType.CyclicEquals(other.InnerType, ref context))
                return false;

            return true;
        }

        public override SymbolQueryResult? QueryMember(Name name, int typeArgCount)
        {
            return null;
        }
    }
}
