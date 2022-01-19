using Gum.Collections;
using Gum.CompileTime;
using System;

namespace Gum.Analysis
{
    // "void"
    public class VoidSymbol : ITypeSymbol
    {
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal VoidSymbol() { }

        public ITypeSymbol Apply(TypeEnv typeEnv)
        {
            return this;
        }
        
        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            throw new NotImplementedException(); // void는 decl이 없다
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
