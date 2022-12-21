using Citron.Collections;
using Citron.Infra;
using Citron.Module;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    public class StructConstructorDeclBuilder<TOuterBuilder>
    {
        TOuterBuilder outerBuilder;
        ImmutableArray<StructConstructorDeclSymbol>.Builder constructorDeclsBuilder;
        IHolder<StructDeclSymbol> outerHolder;
        Accessor accessModifier;
        bool bTrivial;

        Holder<StructConstructorDeclSymbol> constructorDeclHolder;

        FuncRetParamsBuilderComponent<StructConstructorDeclBuilder<TOuterBuilder>> funcRetParamsComponent;
        LambdaBuilderComponent<StructConstructorDeclBuilder<TOuterBuilder>> lambdaComponent;

        internal StructConstructorDeclBuilder(
            TOuterBuilder outerBuilder, 
            ImmutableArray<StructConstructorDeclSymbol>.Builder constructorDeclsBuilder, 
            SymbolFactory factory,
            IHolder<StructDeclSymbol> outerHolder, 
            Accessor accessModifier,
            bool bTrivial)
        {
            this.outerBuilder = outerBuilder;
            this.constructorDeclsBuilder = constructorDeclsBuilder;
            this.outerHolder = outerHolder;
            this.accessModifier = accessModifier;
            this.bTrivial = bTrivial;

            this.constructorDeclHolder = new Holder<StructConstructorDeclSymbol>();
            this.funcRetParamsComponent = new FuncRetParamsBuilderComponent<StructConstructorDeclBuilder<TOuterBuilder>>(this);
            this.lambdaComponent = new LambdaBuilderComponent<StructConstructorDeclBuilder<TOuterBuilder>>(factory, this, constructorDeclHolder);
        }

        public StructConstructorDeclBuilder<TOuterBuilder> Lambda(out LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.Lambda(out lambdaDecl);

        public StructConstructorDeclBuilder<TOuterBuilder> Lambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams, out LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.Lambda(funcRet, funcParams, out lambdaDecl);

        public LambdaDeclBuilder<StructConstructorDeclBuilder<TOuterBuilder>> BeginLambda()
            => lambdaComponent.BeginLambda();

        public LambdaDeclBuilder<StructConstructorDeclBuilder<TOuterBuilder>> BeginLambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams)
            => lambdaComponent.BeginLambda(funcRet, funcParams);

        // embed FuncRetParamsBuilderComponent
        public StructConstructorDeclBuilder<TOuterBuilder> FuncReturn(bool isRef, ITypeSymbol type)
            => funcRetParamsComponent.FuncReturn(isRef, type);

        public StructConstructorDeclBuilder<TOuterBuilder> FuncReturnHolder(out Holder<FuncReturn> funcRetHolder)
            => funcRetParamsComponent.FuncReturnHolder(out funcRetHolder);

        public StructConstructorDeclBuilder<TOuterBuilder> FuncParameter(FuncParameterKind kind, ITypeSymbol type, Name name)
            => funcRetParamsComponent.FuncParameter(kind, type, name);

        public StructConstructorDeclBuilder<TOuterBuilder> FuncParametersHolder(ImmutableArray<FuncParamId> paramIds, out Holder<ImmutableArray<FuncParameter>> funcParamsHolder)
            => funcRetParamsComponent.FuncParametersHolder(paramIds, out funcParamsHolder);

        public TOuterBuilder EndConstructor(out StructConstructorDeclSymbol constructorDecl)
        {
            var paramsHolder = funcRetParamsComponent.GetParamsOnly();
            var lambdaDecls = lambdaComponent.MakeLambdaDecls();

            constructorDecl = new StructConstructorDeclSymbol(outerHolder, accessModifier, paramsHolder, bTrivial, lambdaDecls);
            constructorDeclHolder.SetValue(constructorDecl);
            constructorDeclsBuilder.Add(constructorDecl);

            return outerBuilder;
        }
    }
}