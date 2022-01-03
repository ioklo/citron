using Gum.Collections;
using Pretune;
using System;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class StructMemberFuncDeclSymbol : IDeclSymbolNode
    {
        Lazy<StructDeclSymbol> outer;

        M.AccessModifier accessModifier;
        bool bInstanceFunc;
        FuncReturn @return;
        M.Name name;
        ImmutableArray<string> typeParams;
        ImmutableArray<FuncParameter> parameters;        

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeMParamTypes());
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.Value;
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public bool IsInstanceFunc()
        {
            return bInstanceFunc;
        }        

        public R.Path.Nested MakeRPath(R.Path.Normal outerPath, ImmutableArray<R.Path> typeArgs)
        {
            var rname = RItemFactory.MakeName(name);
            var paramHash = new R.ParamHash(typeArgs.Length, parameters.MakeParamHashEntries());            

            return new R.Path.Nested(outerPath, rname, paramHash, typeArgs);
        }
    }
}