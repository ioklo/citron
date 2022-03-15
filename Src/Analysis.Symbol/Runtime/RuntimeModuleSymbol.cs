using Citron.CompileTime;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public class RuntimeModuleDeclSymbol : ITopLevelDeclSymbolNode
    {
        SystemNamespaceDeclSymbol systemNamespaceDecl;

        public RuntimeModuleDeclSymbol()
        {
            systemNamespaceDecl = new SystemNamespaceDeclSymbol(this);
        }

        void IDeclSymbolNode.Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitRuntimeModule(this);
        }

        AccessModifier IDeclSymbolNode.GetAccessModifier()
        {
            return AccessModifier.Public;
        }

        IEnumerable<IDeclSymbolNode> IDeclSymbolNode.GetMemberDeclNodes()
        {
            yield return systemNamespaceDecl;
        }

        DeclSymbolNodeName IDeclSymbolNode.GetNodeName()
        {
            return new DeclSymbolNodeName(new Name.Normal("System.Runtime"), 0, default);
        }

        IDeclSymbolNode? IDeclSymbolNode.GetOuterDeclNode()
        {
            return null;
        }
    }

    public class RuntimeModuleSymbol : ITopLevelSymbolNode
    {
        RuntimeModuleDeclSymbol decl;

        ITopLevelSymbolNode ITopLevelSymbolNode.Apply(TypeEnv typeEnv)
        {
            return this;
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv)
        {
            return this;
        }

        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode()
        {
            return decl;
        }

        ISymbolNode? ISymbolNode.GetOuter()
        {
            return null;
        }

        //(Name Module, NamespacePath? NamespacePath) ITopLevelSymbolNode.GetRootPath()
        //{
        //    return new Name.Normal("")
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