using Citron.Collections;
using Citron.CompileTime;
using System;

namespace Citron.Analysis
{
    // "void"
    public class VoidSymbol : ITypeSymbol
    {
        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal VoidSymbol() { }

        public ITypeSymbol Apply(TypeEnv typeEnv)
        {
            return this;
        }
        
        public ITypeDeclSymbol? GetDeclSymbolNode()
        {
            return null;
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitVoid(this);
        }

        public SymbolQueryResult QueryMember(Name memberName, int typeParamCount)
        {
            return SymbolQueryResult.NotFound.Instance;
        }

        public ISymbolNode? GetOuter()
        {
            return null;
        }

        public TypeEnv GetTypeEnv()
        {
            return default;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }
    }
}
