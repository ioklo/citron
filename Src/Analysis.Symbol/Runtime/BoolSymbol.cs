using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public class BoolDeclSymbol : ITypeDeclSymbol
    {
        SystemNamespaceDeclSymbol outer;

        public BoolDeclSymbol(SystemNamespaceDeclSymbol outer)
        {
            this.outer = outer;
        }

        void ITypeDeclSymbol.Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitBool(this);
        }

        void IDeclSymbolNode.Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitBool(this);
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
            return new DeclSymbolNodeName(new Name.Normal("Boolean"), 0, default);
        }

        IDeclSymbolNode? IDeclSymbolNode.GetOuterDeclNode()
        {
            return outer;
        }
    }

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

        public ITypeSymbol GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
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