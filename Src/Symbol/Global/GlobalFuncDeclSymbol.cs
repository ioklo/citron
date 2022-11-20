using Citron.Collections;
using Pretune;
using System;
using Citron.Module;
using Citron.Infra;
using System.Linq;
using System.Collections.Generic;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class GlobalFuncDeclSymbol : IFuncDeclSymbol
    {
        // module or namespace
        IHolder<ITopLevelDeclSymbolNode> outerHolder;

        AccessModifier accessModifier;
        IHolder<FuncReturn> returnHolder;
        Name name;

        ImmutableArray<TypeVarDeclSymbol> typeParams;
        IHolder<ImmutableArray<FuncParameter>> parametersHolder;
        
        bool bInternal;

        LambdaDeclSymbolContainerComponent lambdaDeclContainer;

        public void AddLambda(LambdaDeclSymbol lambdaDecl)
            => lambdaDeclContainer.AddLambda(lambdaDecl);

        public GlobalFuncDeclSymbol(
            IHolder<ITopLevelDeclSymbolNode> outerHolder, AccessModifier accessModifier, 
            IHolder<FuncReturn> returnHolder,
            Name name,            
            ImmutableArray<TypeVarDeclSymbol> typeParams,
            IHolder<ImmutableArray<FuncParameter>> parametersHolder, 
            bool bInternal, 
            ImmutableArray<LambdaDeclSymbol> lambdaDecls)
        {
            this.outerHolder = outerHolder;
            this.accessModifier = accessModifier;
            this.returnHolder = returnHolder;
            this.name = name;
            this.typeParams = typeParams;
            this.parametersHolder = parametersHolder;
            this.bInternal = bInternal;
            this.lambdaDeclContainer = new LambdaDeclSymbolContainerComponent(lambdaDecls);
        }

        public AccessModifier GetAccessModifier()
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
        
        public bool IsInternal()
        {
            return bInternal;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return typeParams.AsEnumerable().OfType<IDeclSymbolNode>()
                .Concat(lambdaDeclContainer.GetLambdaDecls());
        }        

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitGlobalFunc(this);
        }
    }
}