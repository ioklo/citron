using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using Citron.Module;

namespace Citron.Symbol
{
    // typeDecl에 대리
    [AutoConstructor]
    public partial class GlobalTypeDeclSymbol : ITypeDeclSymbolContainer, TypeDict.IHaveNodeName
    {
        IHolder<ITopLevelDeclSymbolNode> outerHolder;
        AccessModifier accessModifier;
        ITypeDeclSymbol typeDecl;

        IDeclSymbolNode ITypeDeclSymbolContainer.GetOuterDeclNode() => GetOuter();

        public ITopLevelDeclSymbolNode GetOuter()
        {
            return outerHolder.GetValue();
        }

        public AccessModifier GetAccessModifier() 
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