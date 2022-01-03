using Gum.Collections;
using Gum.Infra;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class StructMemberTypeDeclSymbol : IDeclSymbolNode
    {
        IHolder<StructDeclSymbol> outer;
        ITypeDeclSymbolNode typeDecl;

        public StructMemberTypeDeclSymbol(IHolder<StructDeclSymbol> outer, ITypeDeclSymbolNode typeDecl)
        {
            this.outer = outer;
            this.typeDecl = typeDecl;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return typeDecl.GetNodeName();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return typeDecl.GetMemberDeclNode(name, typeParamCount, paramTypes);
        }
        
        public int GetTypeParamCount()
        {
            return typeDecl.GetTypeParamCount();
        }

        public void Apply(ITypeDeclSymbolNodeVisitor visitor)
        {
            typeDecl.Apply(visitor);
        }
    }
}