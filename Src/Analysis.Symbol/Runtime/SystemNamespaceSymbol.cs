using Citron.CompileTime;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public class SystemNamespaceDeclSymbol : ITopLevelDeclSymbolNode
    {
        RuntimeModuleDeclSymbol outer;

        BoolDeclSymbol boolDecl;
        IntDeclSymbol intDecl;
        StringDeclSymbol stringDecl;
        ListDeclSymbol listDecl;

        public SystemNamespaceDeclSymbol(RuntimeModuleDeclSymbol outer)
        {
            this.outer = outer;
            this.boolDecl = new BoolDeclSymbol(this);
            this.intDecl = new IntDeclSymbol(this);
            this.stringDecl = new StringDeclSymbol(this);
            this.listDecl = new ListDeclSymbol(this);
        }

        void IDeclSymbolNode.Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitSystemNamespace(this);
        }

        AccessModifier IDeclSymbolNode.GetAccessModifier()
        {
            return AccessModifier.Public;
        }

        IEnumerable<IDeclSymbolNode> IDeclSymbolNode.GetMemberDeclNodes()
        {
            yield return boolDecl;
            yield return intDecl;
            yield return stringDecl;
            yield return listDecl;
        }

        DeclSymbolNodeName IDeclSymbolNode.GetNodeName()
        {
            return new DeclSymbolNodeName(new Name.Normal("System"), 0, default);
        }

        IDeclSymbolNode? IDeclSymbolNode.GetOuterDeclNode()
        {
            return outer;
        }
    }

    public class SystemNamespaceSymbol : ITopLevelSymbolNode
    {
        ITopLevelSymbolNode ITopLevelSymbolNode.Apply(TypeEnv typeEnv)
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

        ISymbolNode? ISymbolNode.GetOuter()
        {
            throw new System.NotImplementedException();
        }

        //(Name Module, NamespacePath? NamespacePath) ITopLevelSymbolNode.GetRootPath()
        //{
        //    throw new System.NotImplementedException();
        //}

        ITypeSymbol ISymbolNode.GetTypeArg(int i)
        {
            throw new System.NotImplementedException();
        }

        TypeEnv ISymbolNode.GetTypeEnv()
        {
            throw new System.NotImplementedException();
        }

        SymbolQueryResult ITopLevelSymbolNode.QueryMember(Name memberName, int typeParamCount)
        {
            throw new System.NotImplementedException();
        }
    }
}