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
}
