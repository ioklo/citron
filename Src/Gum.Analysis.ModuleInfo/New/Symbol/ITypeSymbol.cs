using Gum.Collections;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface ITypeSymbol : ISymbolNode
    {
        new ITypeSymbol Apply(TypeEnv typeEnv);
        new ITypeDeclSymbol? GetDeclSymbolNode();

        void Apply(ITypeSymbolVisitor visitor);

        SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount);
    }

    public static class TypeSymbolExtensions
    {
        public static DeclSymbolId? GetDeclSymbolId(this ITypeSymbol type)
        {
            var decl = type.GetDeclSymbolNode();
            if (decl == null) return null;

            return decl.GetDeclSymbolId();
        }
    }
}