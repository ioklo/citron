using Gum.Collections;
using Gum.Infra;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class StructMemberTypeDeclSymbol : IDeclSymbolNode
    {
        M.AccessModifier accessModifier;
        ITypeDeclSymbolNode typeDecl;

        public StructMemberTypeDeclSymbol(M.AccessModifier accessModifier, ITypeDeclSymbolNode typeDecl)
        {
            this.accessModifier = accessModifier;
            this.typeDecl = typeDecl;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return typeDecl.GetNodeName();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return typeDecl.GetOuterDeclNode();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return typeDecl.GetMemberDeclNode(name, typeParamCount, paramIds);
        }
        
        public int GetTypeParamCount()
        {
            return typeDecl.GetTypeParamCount();
        }

        public void Apply(ITypeDeclSymbolNodeVisitor visitor)
        {
            typeDecl.Apply(visitor);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            typeDecl.Apply(visitor);
        }
    }
}