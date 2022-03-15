using Citron.Collections;
using System;

using M = Citron.CompileTime;
using Citron.Infra;
using System.Collections.Generic;
using System.Linq;

namespace Citron.Analysis
{
    public record ClassConstructorDeclSymbol : IFuncDeclSymbol
    {
        IHolder<ClassDeclSymbol> outer;
        M.AccessModifier accessModifier;
        IHolder<ImmutableArray<FuncParameter>> parameters;
        bool bTrivial;
        LambdaDeclSymbolContainerComponent lambdaDeclContainerComponent;

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
            => lambdaDeclContainerComponent.AddLambda(lambdaDecl);

        public ClassConstructorDeclSymbol(IHolder<ClassDeclSymbol> outer, M.AccessModifier accessModifier, IHolder<ImmutableArray<FuncParameter>> parameters, bool bTrivial, ImmutableArray<LambdaDeclSymbol> lambdaDecls)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parameters = parameters;
            this.bTrivial = bTrivial;

            this.lambdaDeclContainerComponent = new LambdaDeclSymbolContainerComponent(lambdaDecls);
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
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassConstructor(this);
        }
    }
}
