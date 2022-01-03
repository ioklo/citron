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
        ImmutableArray<FuncParameter> parameters;

        internal StructConstructorDeclSymbol(IHolder<StructDeclSymbol> outer, M.AccessModifier accessModifier, ImmutableArray<FuncParameter> parameters)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parameters = parameters;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(M.Name.Constructor, 0, parameters.MakeMParamTypes());
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        public R.Path.Nested MakeRPath(R.Path.Normal outer)
        {
            var paramHash = new R.ParamHash(0, parameters.MakeParamHashEntries());
            return new R.Path.Nested(outer, R.Name.Constructor.Instance, paramHash, default);
        }

        public int GetParameterCount()
        {
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters[index];
        }
    }
}