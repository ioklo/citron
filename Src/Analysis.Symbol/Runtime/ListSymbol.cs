using Citron.Collections;
using Citron.CompileTime;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public class ListDeclSymbol : ITypeDeclSymbol
    {
        SystemNamespaceDeclSymbol outer;

        public ListDeclSymbol(SystemNamespaceDeclSymbol outer)
        {
            this.outer = outer;
        }

        void ITypeDeclSymbol.Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitList(this);
        }

        void IDeclSymbolNode.Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitList(this);
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
            return new DeclSymbolNodeName(new Name.Normal("List"), 0, default);
        }

        IDeclSymbolNode? IDeclSymbolNode.GetOuterDeclNode()
        {
            return outer;
        }
    }

    public class ListSymbol : ITypeSymbol
    {
        SymbolFactory factory;
        ITypeSymbol itemType;

        internal ListSymbol(SymbolFactory factory, ITypeSymbol itemType)
        {
            this.factory = factory;
            this.itemType = itemType;
        }

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