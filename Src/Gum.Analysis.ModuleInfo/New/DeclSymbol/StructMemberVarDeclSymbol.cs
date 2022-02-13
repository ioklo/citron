using System;

using Gum.Collections;
using Gum.Infra;
using Pretune;

using M = Gum.CompileTime;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class StructMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<StructDeclSymbol> outer;

        M.AccessModifier accessModifier;
        bool bStatic;
        IHolder<ITypeSymbol> declTypeHolder;
        M.Name name;

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            return null;
        }

        public M.Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public ITypeSymbol GetDeclType()
        {
            return declTypeHolder.GetValue();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberVar(this);
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}