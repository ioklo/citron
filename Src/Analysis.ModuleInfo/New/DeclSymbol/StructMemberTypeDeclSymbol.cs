using System;

using Citron.Collections;
using Citron.Infra;
using Pretune;

using M = Citron.CompileTime;

namespace Citron.Analysis
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

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}