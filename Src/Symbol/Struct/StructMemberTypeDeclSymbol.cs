using System;

using Citron.Collections;
using Citron.Infra;
using Pretune;

using Citron.Module;

namespace Citron.Symbol
{
    [AutoConstructor]
    public partial class StructMemberTypeDeclSymbol : ITypeDeclSymbolContainer, TypeDict.IHaveNodeName
    {
        IHolder<StructDeclSymbol> outerHolder;
        AccessModifier accessModifier;
        ITypeDeclSymbol typeDecl;

        IDeclSymbolNode ITypeDeclSymbolContainer.GetOuterDeclNode() => GetOuter();

        public StructDeclSymbol GetOuter()
        {
            return outerHolder.GetValue();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return typeDecl.GetNodeName();
        }
        
        public int GetTypeParamCount()
        {
            return typeDecl.GetTypeParamCount();
        }

        public IDeclSymbolNode GetNode()
        {
            return typeDecl;
        }

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            typeDecl.Apply(visitor);
        }

        public AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}