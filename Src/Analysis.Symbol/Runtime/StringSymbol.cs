using Citron.Collections;
using Citron.CompileTime;

namespace Citron.Analysis
{
    public class StringSymbol : ITypeSymbol
    {
        public ITypeSymbol Apply(TypeEnv typeEnv)
        {
            throw new System.NotImplementedException();
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            throw new System.NotImplementedException();
        }

        public ITypeDeclSymbol? GetDeclSymbolNode()
        {
            throw new System.NotImplementedException();
        }

        public ISymbolNode? GetOuter()
        {
            throw new System.NotImplementedException();
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            throw new System.NotImplementedException();
        }

        public TypeEnv GetTypeEnv()
        {
            throw new System.NotImplementedException();
        }

        public SymbolQueryResult QueryMember(Name memberName, int typeParamCount)
        {
            throw new System.NotImplementedException();
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv)
        {
            throw new System.NotImplementedException();
        }

        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode()
        {
            throw new System.NotImplementedException();
        }
    }
}