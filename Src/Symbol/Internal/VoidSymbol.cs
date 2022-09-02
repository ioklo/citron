using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;

namespace Citron.Symbol
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
            return SymbolQueryResults.NotFound;
        }

        public ISymbolNode? GetOuter()
        {
            return null;
        }

        public TypeEnv GetTypeEnv()
        {
            return default;
        }

        public ITypeSymbol GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }
    }
}
