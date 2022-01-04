using Gum.Collections;
using Gum.Infra;
using System;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public class StructConstructorDeclSymbol : IDeclSymbolNode
    {
        IHolder<StructDeclSymbol> outer;
        M.AccessModifier accessModifier;
        IHolder<ImmutableArray<FuncParameter>> parametersHolder;
        bool bTrivial;

        public StructConstructorDeclSymbol(IHolder<StructDeclSymbol> outer, M.AccessModifier accessModifier, IHolder<ImmutableArray<FuncParameter>> parametersHolder, bool bTrivial)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parametersHolder = parametersHolder;
            this.bTrivial = bTrivial;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(M.Name.Constructor, 0, parametersHolder.GetValue().MakeMParamTypes());
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        public R.Path.Nested MakeRPath(R.Path.Normal outer)
        {
            var paramHash = new R.ParamHash(0, parametersHolder.GetValue().MakeParamHashEntries());
            return new R.Path.Nested(outer, R.Name.Constructor.Instance, paramHash, default);
        }

        public int GetParameterCount()
        {
            return parametersHolder.GetValue().Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parametersHolder.GetValue()[index];
        }
    }
}