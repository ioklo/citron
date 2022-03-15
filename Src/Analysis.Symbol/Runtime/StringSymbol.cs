using Citron.Collections;
using Citron.CompileTime;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public class StringDeclSymbol : ITypeDeclSymbol
    {
        SystemNamespaceDeclSymbol outer;

        public StringDeclSymbol(SystemNamespaceDeclSymbol outer)
        {
            this.outer = outer;
        }

        void ITypeDeclSymbol.Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitString(this);
        }

        void IDeclSymbolNode.Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitString(this);
        }

        AccessModifier IDeclSymbolNode.GetAccessModifier()
        {
            return AccessModifier.Public;
        }

        IEnumerable<IDeclSymbolNode> IDeclSymbolNode.GetMemberDeclNodes()
        {
            throw new System.NotImplementedException();
        }

        DeclSymbolNodeName IDeclSymbolNode.GetNodeName()
        {
            return new DeclSymbolNodeName(new Name.Normal("String"), 0, default);
        }

        IDeclSymbolNode? IDeclSymbolNode.GetOuterDeclNode()
        {
            return outer;
        }
    }

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

        public ITypeSymbol GetTypeArg(int index)
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