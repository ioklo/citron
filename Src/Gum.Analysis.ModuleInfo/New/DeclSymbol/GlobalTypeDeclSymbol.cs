using Gum.Collections;
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

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return typeDecl.GetMemberDeclNode(name, typeParamCount, paramIds);
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return typeDecl.GetNodeName();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            typeDecl.Apply(visitor);
        }
    }
}