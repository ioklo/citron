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
    public interface IType : ICyclicEqualityComparableClass<IType>
    {
        IType Apply(TypeEnv typeEnv);
        TypeId GetTypeId();
        FuncParamTypeId MakeFuncParamTypeId();
        IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs); // 이름에 해당하는 멤버타입을 가져온다
        SymbolQueryResult QueryMember(string idName, int typeArgCount);

        void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ITypeVisitor;
    }

    public abstract record class TypeImpl : IType
    {
        public abstract IType Apply(TypeEnv typeEnv);
        public abstract TypeId GetTypeId();
        public abstract FuncParamTypeId MakeFuncParamTypeId();
        public abstract IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs);
        public abstract void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ITypeVisitor;

        public abstract bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context);
        public abstract SymbolQueryResult QueryMember(string idName, int typeArgCount);
    }

    public abstract record class SymbolType : TypeImpl
    {
        protected abstract ITypeSymbol GetTypeSymbol();

        public sealed override TypeId GetTypeId() => GetTypeSymbol().GetSymbolId();
        public sealed override FuncParamTypeId MakeFuncParamTypeId() => new FuncParamTypeId.Symbol(GetTypeSymbol().GetSymbolId());
        public sealed override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => GetTypeSymbol().GetMemberType(name, typeArgs);

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
        {
            return other is SymbolType otherSymbolType && GetTypeSymbol().CyclicEquals(otherSymbolType.GetTypeSymbol(), ref context);
        }

        public sealed override SymbolQueryResult QueryMember(string idName, int typeArgCount)
        {
            return GetTypeSymbol().QueryMember(new Name.Normal(idName), typeArgCount);
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
        public override FuncParamTypeId MakeFuncParamTypeId() => new FuncParamTypeId.Nullable(InnerType.MakeFuncParamTypeId());
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

        public override SymbolQueryResult QueryMember(string idName, int typeArgCount)
        {
            return SymbolQueryResults.NotFound;
        }
    }

    public record class TypeVarType(int Index, Name Name) : TypeImpl
    {
        public override IType Apply(TypeEnv typeEnv) => typeEnv.GetValue(Index);
        public override TypeId GetTypeId() => new TypeVarTypeId(Index, Name);
        public override FuncParamTypeId MakeFuncParamTypeId() => new FuncParamTypeId.TypeVar(Index);
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

        public override SymbolQueryResult QueryMember(string idName, int typeArgCount)
        {
            return SymbolQueryResults.NotFound;
        }
    }

    public record class VoidType: TypeImpl
    {
        public override VoidType Apply(TypeEnv typeEnv) => new VoidType();
        public override TypeId GetTypeId() => new VoidTypeId();
        public override FuncParamTypeId MakeFuncParamTypeId() => new FuncParamTypeId.Void();
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitVoid(this);

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => true;

        public override SymbolQueryResult QueryMember(string idName, int typeArgCount)
        {
            return SymbolQueryResults.NotFound;
        }
    }

    public record class TupleType(ImmutableArray<(IType Type, Name Name)> MemberVars) : TypeImpl
    {
        public override IType Apply(TypeEnv typeEnv)
        {
            var builder = ImmutableArray.CreateBuilder<(IType, Name)>(MemberVars.Length);
            foreach (var memberVar in MemberVars)
                builder.Add((memberVar.Type.Apply(typeEnv), memberVar.Name));
            return new TupleType(builder.MoveToImmutable());
        }

        public override TypeId GetTypeId()
        {
            var builder = ImmutableArray.CreateBuilder<(TypeId, Name)>(MemberVars.Length);
            foreach (var memberVar in MemberVars)
                builder.Add((memberVar.Type.GetTypeId(), memberVar.Name));
            return new TupleTypeId(builder.MoveToImmutable());
        }

        public override FuncParamTypeId MakeFuncParamTypeId()
        {
            var builder = ImmutableArray.CreateBuilder<FuncParamTypeId>(MemberVars.Length);
            foreach (var memberVar in MemberVars)
                builder.Add(memberVar.Type.MakeFuncParamTypeId());
            return new FuncParamTypeId.Tuple(builder.MoveToImmutable());
        }

        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitTuple(this);

        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => other is TupleType otherType && CyclicEquals(otherType, ref context);

        bool CyclicEquals(TupleType other, ref CyclicEqualityCompareContext context)
        {
            int memberVarCount = MemberVars.Length;
            int otherMemberVarCount = other.MemberVars.Length;

            if (memberVarCount == 0 && otherMemberVarCount == 0) return true;
            if (memberVarCount == 0 || otherMemberVarCount == 0) return false;

            if (memberVarCount != otherMemberVarCount) return false;
            
            for (int i = 0; i < memberVarCount; i++)
            {
                if (!context.CompareClass(MemberVars[i].Type, other.MemberVars[i].Type))
                    return false;

                if (!MemberVars[i].Name.Equals(other.MemberVars[i].Name))
                    return false;
            }

            return true;
        }

        public override SymbolQueryResult QueryMember(string idName, int typeArgCount)
        {
            foreach (var memberVar in MemberVars)
                if (memberVar.Name.Equals(new Name.Normal(idName)))
                    return new SymbolQueryResult.TupleMemberVar();

            return SymbolQueryResults.NotFound;
        }
    }

    public record class VarType : TypeImpl
    {
        public override IType Apply(TypeEnv typeEnv) => new VarType();
        public override TypeId GetTypeId() => new VarTypeId();
        public override FuncParamTypeId MakeFuncParamTypeId() => throw new NotImplementedException(); // var는 함수 이름으로 사용할 수 없습니다 에러 작성
        public override IType? GetMemberType(Name name, ImmutableArray<IType> typeArgs) => null;
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitVar(this);
        public sealed override bool CyclicEquals(IType other, ref CyclicEqualityCompareContext context)
            => true;

        public override SymbolQueryResult QueryMember(string idName, int typeArgCount)
        {
            return SymbolQueryResults.NotFound;
        }
    }
}
