using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class StructMemberTypeDeclSymbol : ITypeDeclSymbolContainer, TypeDict.IHaveNodeName
    {
        IHolder<StructDeclSymbol> outerHolder;
        M.AccessModifier accessModifier;
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

        
        
    }
}