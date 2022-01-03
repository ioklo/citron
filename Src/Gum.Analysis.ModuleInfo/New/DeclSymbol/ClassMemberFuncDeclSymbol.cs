using System;
using Gum.Collections;
using R = Gum.IR0;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public record ClassMemberFuncDeclSymbol : IDeclSymbolNode
    {
        Lazy<ClassDeclSymbol> outer;
        M.AccessModifier accessModifier;
        FuncReturn @return;
        M.Name name;
        ImmutableArray<string> typeParams;
        ImmutableArray<FuncParameter> parameters;
        bool bInstanceFunc;

        public ClassMemberFuncDeclSymbol(
            Lazy<ClassDeclSymbol> outer, 
            M.AccessModifier accessModifier, 
            FuncReturn @return,
            M.Name name,
            ImmutableArray<string> typeParams,
            ImmutableArray<FuncParameter> parameters,
            bool bInstanceFunc)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.@return = @return;
            this.name = name;
            this.typeParams = typeParams;
            this.parameters = parameters;
            this.bInstanceFunc = bInstanceFunc;
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

        public FuncReturn GetReturn()
        {
            return @return;
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public bool IsInstanceFunc()
        {
            return bInstanceFunc;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeMParamTypes());
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.Value;
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        public R.Path.Nested MakeRPath(R.Path.Nested outerPath, ImmutableArray<R.Path> typeArgs)
        {
            var rname = RItemFactory.MakeName(name);
            var paramHash = new R.ParamHash(typeArgs.Length, parameters.MakeParamHashEntries());
            return new R.Path.Nested(outerPath, rname, paramHash, typeArgs);
        }
    }
}
