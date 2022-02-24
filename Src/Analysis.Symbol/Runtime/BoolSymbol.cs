using Citron.Collections;
using Citron.CompileTime;

namespace Citron.Analysis
{
    // [System.Runtime] System.Boolean proxy
    public class BoolSymbol : ITypeSymbol
    {
        public ITypeSymbol Apply(TypeEnv typeEnv)
        {
            return this;
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            // StructSymbol이 대신 해줘야 한다
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