using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
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

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
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

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberFunc(this);
        }

        public FuncReturn GetReturn()
        {
            return returnHolder.GetValue();
        }
    }
}