using Gum.Collections;
using System;

using M = Gum.CompileTime;
using Gum.Infra;

namespace Citron.Analysis
{
    public record ClassConstructorDeclSymbol : IFuncDeclSymbol
    {
        IHolder<ClassDeclSymbol> outer;
        M.AccessModifier accessModifier;
        IHolder<ImmutableArray<FuncParameter>> parameters;
        bool bTrivial;

        public ClassConstructorDeclSymbol(IHolder<ClassDeclSymbol> outer, M.AccessModifier accessModifier, IHolder<ImmutableArray<FuncParameter>> parameters, bool bTrivial)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parameters = parameters;
            this.bTrivial = bTrivial;
        }

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }

        public int GetParameterCount()
        {
            return parameters.GetValue().Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters.GetValue()[index];
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(M.Name.Constructor, 0, parameters.GetValue().MakeFuncParamIds());
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            return null;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassConstructor(this);
        }
    }
}
