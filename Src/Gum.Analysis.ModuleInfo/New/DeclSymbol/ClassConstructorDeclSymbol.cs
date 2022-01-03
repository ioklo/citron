using Gum.Collections;
using System;

using R = Gum.IR0;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public record ClassConstructorDeclSymbol : IDeclSymbolNode
    {
        Lazy<ClassDeclSymbol> outer;
        M.AccessModifier accessModifier;
        ImmutableArray<FuncParameter> parameters;

        public ClassConstructorDeclSymbol(Lazy<ClassDeclSymbol> outer, M.AccessModifier accessModifier, ImmutableArray<FuncParameter> parameters)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parameters = parameters;
        }

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }

        public int GetParameterCount()
        {
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters[index];
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.Value;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(M.Name.Constructor, 0, parameters.MakeMParamTypes());
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        // typeArgs가 없다
        public R.Path.Nested MakeRPath(R.Path.Normal outerPath)
        {   
            var paramHash = new R.ParamHash(0, parameters.MakeParamHashEntries());
            return new R.Path.Nested(outerPath, R.Name.Constructor.Instance, paramHash, default);
        }
    }
}
