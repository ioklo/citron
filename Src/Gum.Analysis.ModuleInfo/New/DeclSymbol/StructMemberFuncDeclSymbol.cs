using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class StructMemberFuncDeclSymbol : IDeclSymbolNode
    {
        IHolder<StructDeclSymbol> outer;

        M.AccessModifier accessModifier;
        bool bStatic;
        IHolder<FuncReturn> returnHolder;
        M.Name name;
        ImmutableArray<string> typeParams;
        IHolder<ImmutableArray<FuncParameter>> parametersHolder;


        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, parametersHolder.GetValue().MakeMParamTypes());
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public bool IsStatic()
        {
            return bStatic;
        }        

        public R.Path.Nested MakeRPath(R.Path.Normal outerPath, ImmutableArray<R.Path> typeArgs)
        {
            var rname = RItemFactory.MakeName(name);
            var paramHash = new R.ParamHash(typeArgs.Length, parametersHolder.GetValue().MakeParamHashEntries());            

            return new R.Path.Nested(outerPath, rname, paramHash, typeArgs);
        }
    }
}