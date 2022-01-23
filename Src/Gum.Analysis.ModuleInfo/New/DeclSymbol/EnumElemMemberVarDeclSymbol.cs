using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class EnumElemMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<EnumElemDeclSymbol> outerHolder;
        IHolder<ITypeSymbol> declTypeHolder;
        M.Name name;

        public M.Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return null;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }

        public ITypeSymbol GetDeclType()
        {
            return declTypeHolder.GetValue();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnumElemMemberVar(this);
        }

        public M.AccessModifier GetAccessModifier()
        {
            return M.AccessModifier.Public;
        }
    }
}