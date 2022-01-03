using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // typeDecl에 대리
    [AutoConstructor]
    public partial class GlobalTypeDeclSymbol : IDeclSymbolNode
    {
        ITypeDeclSymbolNode typeDecl;

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