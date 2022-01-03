using Gum.Collections;
using System;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // 일단 M.-는 허용, 확장할 일이 있으면 감싸서 만든다
    public record ClassMemberTypeDeclSymbol : IDeclSymbolNode
    {
        M.AccessModifier accessModifier;        
        ITypeDeclSymbolNode typeDecl;

        public ClassMemberTypeDeclSymbol(M.AccessModifier accessModifier, ITypeDeclSymbolNode typeDecl)
        {
            this.accessModifier = accessModifier;
            this.typeDecl = typeDecl;
        }

        public DeclSymbolNodeName GetNodeName() 
        {
            return typeDecl.GetNodeName();
        }

        public int GetTypeParamCount()
        {
            return typeDecl.GetTypeParamCount();
        }

        public void Apply(ITypeDeclSymbolNodeVisitor visitor)
        {
            typeDecl.Apply(visitor);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return typeDecl.GetOuterDeclNode();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return typeDecl.GetMemberDeclNode(name, typeParamCount, paramTypes);
        }
    }
}
