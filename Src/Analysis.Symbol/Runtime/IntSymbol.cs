using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public class IntDeclSymbol : ITypeDeclSymbol
    {
        SystemNamespaceDeclSymbol outer;

        public IntDeclSymbol(SystemNamespaceDeclSymbol outer)
        {
            this.outer = outer;
        }

        void ITypeDeclSymbol.Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitInt(this);
        }

        void IDeclSymbolNode.Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitInt(this);
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
            return new DeclSymbolNodeName(new Name.Normal("Int32"), 0, default);
        }

        IDeclSymbolNode? IDeclSymbolNode.GetOuterDeclNode()
        {
            return outer;
        }
    }

    // Runtime의 주소는 [System.Runtime]System.Int32
    public class IntSymbol : ITypeSymbol
    {
        SystemNamespaceSymbol outer;
        IntDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();

        IntSymbol(SystemNamespaceSymbol outer, IntDeclSymbol decl)
        {
            this.outer = outer;
            this.decl = decl;
        }

        public ITypeSymbol Apply(TypeEnv typeEnv)
        {
            // int는 type variable이 없으므로 자기 자신을 리턴한다
            return this;
        }
        
        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitInt(this);
        }

        public ITypeDeclSymbol? GetDeclSymbolNode()
        {
            return decl;
        }

        // Runtime의 
        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public ITypeSymbol GetTypeArg(int index)
        {
            // 없다
            throw new RuntimeFatalException();
        }

        public TypeEnv GetTypeEnv()
        {
            return TypeEnv.Empty;
        }

        public SymbolQueryResult QueryMember(Name memberName, int typeParamCount)
        {
            throw new System.NotImplementedException();
        }
    }
}