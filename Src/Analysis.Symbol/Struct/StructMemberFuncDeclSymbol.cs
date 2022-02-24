using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class StructMemberFuncDeclSymbol : IFuncDeclSymbol
    {
        IHolder<StructDeclSymbol> outer;

        M.AccessModifier accessModifier;
        bool bStatic;
        IHolder<FuncReturn> returnHolder;
        M.Name name;
        ImmutableArray<string> typeParams;
        IHolder<ImmutableArray<FuncParameter>> parametersHolder;

        public int GetParameterCount()
        {
            return parametersHolder.GetValue().Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parametersHolder.GetValue()[index];
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, parametersHolder.GetValue().MakeFuncParamIds());
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
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

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberFunc(this);
        }

        public FuncReturn GetReturn()
        {
            return returnHolder.GetValue();
        }

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}