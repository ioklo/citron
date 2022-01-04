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
        IHolder<ITypeSymbolNode> declTypeHolder;
        M.Name name;

        public M.Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }

        public ITypeSymbolNode GetDeclType()
        {
            return declTypeHolder.GetValue();
        }
    }
}