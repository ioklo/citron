using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // typeDecl에 대리
    [AutoConstructor]
    public partial class GlobalTypeDeclSymbol : ITypeDeclSymbolContainer, TypeDict.IHaveNodeName
    {
        IHolder<ITopLevelDeclSymbolNode> outerHolder;
        M.AccessModifier accessModifier;
        ITypeDeclSymbol typeDecl;

        IDeclSymbolNode ITypeDeclSymbolContainer.GetOuterDeclNode() => GetOuter();

        public ITopLevelDeclSymbolNode GetOuter()
        {
            return outerHolder.GetValue();
        }

        public M.AccessModifier GetAccessModifier() 
        { 
            return accessModifier;
        }
        
        public DeclSymbolNodeName GetNodeName()
        {
            return typeDecl.GetNodeName();
        }

        public IDeclSymbolNode GetNode()
        {
            return typeDecl;
        }

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            typeDecl.Apply(visitor);
        }
    }
}