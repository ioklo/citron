using Gum.Analysis;
using Gum.Collections;
using R = Gum.IR0;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    public partial class GlobalFuncDeclSymbol : IDeclSymbolNode
    {
        // module or namespace
        ITopLevelDeclSymbolNode outer;

        M.AccessModifier accessModifier;
        FuncReturn @return;
        M.Name name;

        ImmutableArray<string> typeParams;
        ImmutableArray<FuncParameter> parameters;

        bool bInternal;

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeMParamTypes());
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

        public ImmutableArray <string> GetTypeParams()
        {
            return typeParams;
        }
        
        public bool IsInternal()
        {
            return bInternal;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        // 함수는 자식을 갖지 않는다
        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        public R.Path.Nested MakeRPath(R.Path.Normal outerPath, ImmutableArray<R.Path> typeArgs)
        {
            var rname = RItemFactory.MakeName(name);
            var paramHash = new R.ParamHash(typeArgs.Length, parameters.MakeParamHashEntries());
            return new R.Path.Nested(outerPath, rname, paramHash, typeArgs);
        }
    }
}