using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class StructMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<StructDeclSymbol> outer;

        M.AccessModifier accessModifier;
        bool bStatic;
        ITypeSymbolNode declType;
        M.Name name;

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
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

        public ITypeSymbolNode GetDeclType()
        {
            return declType;
        }
    }
}