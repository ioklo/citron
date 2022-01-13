using Gum.Analysis;
using Gum.Collections;
using R = Gum.IR0;
using Pretune;
using System;
using M = Gum.CompileTime;
using Gum.Infra;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    public partial class GlobalFuncDeclSymbol : IDeclSymbolNode
    {
        // module or namespace
        IHolder<ITopLevelDeclSymbolNode> outerHolder;

        M.AccessModifier accessModifier;
        IHolder<FuncReturn> returnHolder;
        M.Name name;

        ImmutableArray<string> typeParams;
        IHolder<ImmutableArray<FuncParameter>> parametersHolder;

        bool bInternal;

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, parametersHolder.GetValue().MakeFuncParamIds());
        }
        
        public int GetParameterCount()
        {
            return parametersHolder.GetValue().Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parametersHolder.GetValue()[index];
        }

        public FuncReturn GetReturn()
        {
            return returnHolder.GetValue();
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
            return outerHolder.GetValue();
        }

        // 함수는 자식을 갖지 않는다
        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return null;
        }

        public R.Path.Nested MakeRPath(R.Path.Normal outerPath, ImmutableArray<R.Path> typeArgs)
        {
            var rname = RItemFactory.MakeName(name);
            var paramHash = new R.ParamHash(typeArgs.Length, parametersHolder.GetValue().MakeParamHashEntries());
            return new R.Path.Nested(outerPath, rname, paramHash, typeArgs);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitGlobalFunc(this);
        }
    }
}