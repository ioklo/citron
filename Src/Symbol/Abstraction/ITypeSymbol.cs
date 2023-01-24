using Citron.Collections;
using Citron.Infra;
using System.Diagnostics;

namespace Citron.Symbol
{
    public interface ITypeSymbol : ISymbolNode, ICyclicEqualityComparableClass<ITypeSymbol>
    {
        new ITypeSymbol Apply(TypeEnv typeEnv);
        new ITypeDeclSymbol GetDeclSymbolNode();
        void Apply(ITypeSymbolVisitor visitor);

        IType MakeType();
        IType? GetMemberType(Name memberName, ImmutableArray<IType> typeArgs);
        SymbolQueryResult QueryMember(Name memberName, int explicitTypeArgCount);
    }

    public static class TypeSymbolExtensions
    {
        public static DeclSymbolId GetDeclSymbolId(this ITypeSymbol type)
        {
            var decl = type.GetDeclSymbolNode();
            return decl.GetDeclSymbolId();
        }
    }
}