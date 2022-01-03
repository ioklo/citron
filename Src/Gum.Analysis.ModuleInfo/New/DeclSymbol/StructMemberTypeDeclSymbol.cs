using Gum.Collections;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class StructMemberTypeDeclSymbol : IDeclSymbolNode
    {
        Lazy<StructDeclSymbol> outer;
        ITypeDeclSymbolNode typeDecl;

        public StructMemberTypeDeclSymbol(Lazy<StructDeclSymbol> outer, ITypeDeclSymbolNode typeDecl)
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
            return outer.Value;
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