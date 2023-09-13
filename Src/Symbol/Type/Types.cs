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

        TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, ITypeVisitor<TResult>;
    }

    // Interface는 built-in이 있고, Symbol이 있다
    public interface IInterfaceType : IType, ICyclicEqualityComparableClass<IInterfaceType>
    {
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
        public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, ITypeVisitor<TResult>;

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
}
