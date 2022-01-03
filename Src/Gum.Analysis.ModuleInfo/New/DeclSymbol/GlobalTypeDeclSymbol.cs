using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // typeDecl에 대리
    [AutoConstructor]
    public partial class GlobalTypeDeclSymbol : IDeclSymbolNode
    {
        M.AccessModifier accessModifier;
        ITypeDeclSymbolNode typeDecl;

        public M.AccessModifier GetAccessModifier() 
        { 
            return accessModifier;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return typeDecl.GetOuterDeclNode();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return typeDecl.GetMemberDeclNode(name, typeParamCount, paramTypes);
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return typeDecl.GetNodeName();
        }
    }
}