using Citron.Collections;
using System;

using Citron.Infra;
using System.Collections.Generic;
using System.Linq;
using Citron.Module;

namespace Citron.Symbol
{
    public record ClassConstructorDeclSymbol : IFuncDeclSymbol
    {
        IHolder<ClassDeclSymbol> outer;
        AccessModifier accessModifier;
        IHolder<ImmutableArray<FuncParameter>> parameters;
        bool bTrivial;
        LambdaDeclSymbolContainerComponent lambdaDeclContainerComponent;

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
            => lambdaDeclContainerComponent.AddLambda(lambdaDecl);

        public ClassConstructorDeclSymbol(IHolder<ClassDeclSymbol> outer, AccessModifier accessModifier, IHolder<ImmutableArray<FuncParameter>> parameters, bool bTrivial, ImmutableArray<LambdaDeclSymbol> lambdaDecls)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parameters = parameters;
            this.bTrivial = bTrivial;

            this.lambdaDeclContainerComponent = new LambdaDeclSymbolContainerComponent(lambdaDecls);
        }

        public AccessModifier GetAccessModifier()
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
            return new DeclSymbolNodeName(Name.Constructor, 0, parameters.GetValue().MakeFuncParamIds());
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
