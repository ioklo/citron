using Citron.Collections;
using Citron.Infra;
using System.Diagnostics;

namespace Citron.Symbol
{
    public interface ITypeSymbol : ISymbolNode, ICyclicEqualityComparableClass<ITypeSymbol>
    {
        new ITypeSymbol Apply(TypeEnv typeEnv);
        new ITypeDeclSymbol GetDeclSymbolNode();
        new void Accept<TTypeSymbolVisitor>(ref TTypeSymbolVisitor visitor)
            where TTypeSymbolVisitor : ITypeSymbolVisitor;

        IType MakeType(bool bLocalInterface);
        ITypeSymbol? GetMemberType(Name memberName, ImmutableArray<IType> typeArgs);
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