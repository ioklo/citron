using Gum.Collections;
using System;

using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Infra;

namespace Gum.Analysis
{
    public record ClassConstructorDeclSymbol : IDeclSymbolNode
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

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return null;
        }

        // typeArgs가 없다
        public R.Path.Nested MakeRPath(R.Path.Normal outerPath)
        {   
            var paramHash = new R.ParamHash(0, parameters.GetValue().MakeParamHashEntries());
            return new R.Path.Nested(outerPath, R.Name.Constructor.Instance, paramHash, default);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassConstructor(this);
        }
    }
}
