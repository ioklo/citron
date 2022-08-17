using Citron.Collections;
using Citron.Infra;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    public class GlobalFuncDeclBuilder<TOuterBuilder>
    {
        internal delegate GlobalFuncDeclSymbol OnFinish(ImmutableArray<LambdaDeclSymbol> lambdaDecls);

        TOuterBuilder outerBuilder;
        OnFinish onFinish;

        Holder<GlobalFuncDeclSymbol> globalFuncDeclHolder;
        LambdaBuilderComponent<GlobalFuncDeclBuilder<TOuterBuilder>> lambdaComponent;        

        internal GlobalFuncDeclBuilder(SymbolFactory factory, TOuterBuilder outerBuilder, OnFinish onFinish)
        {
            this.outerBuilder = outerBuilder;
            this.onFinish = onFinish;

            this.globalFuncDeclHolder = new Holder<GlobalFuncDeclSymbol>();
            this.lambdaComponent = new LambdaBuilderComponent<GlobalFuncDeclBuilder<TOuterBuilder>>(factory, this, globalFuncDeclHolder);
        }

        public GlobalFuncDeclBuilder<TOuterBuilder> Lambda(out LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.Lambda(out lambdaDecl);

        public GlobalFuncDeclBuilder<TOuterBuilder> Lambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams, out LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.Lambda(funcRet, funcParams, out lambdaDecl);

        public LambdaDeclBuilder<GlobalFuncDeclBuilder<TOuterBuilder>> BeginLambda()
            => lambdaComponent.BeginLambda();

        public LambdaDeclBuilder<GlobalFuncDeclBuilder<TOuterBuilder>> BeginLambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams)
            => lambdaComponent.BeginLambda(funcRet, funcParams);

        // sugar
        public TOuterBuilder EndTopLevelFunc(out GlobalFuncDeclSymbol topLevelFuncDecl)
        {
            topLevelFuncDecl = onFinish.Invoke(lambdaComponent.MakeLambdaDecls());
            globalFuncDeclHolder.SetValue(topLevelFuncDecl);

            return outerBuilder;
        }

        public TOuterBuilder EndGlobalFunc(out GlobalFuncDeclSymbol globalFuncDecl)
        {
            globalFuncDecl = onFinish.Invoke(lambdaComponent.MakeLambdaDecls());
            globalFuncDeclHolder.SetValue(globalFuncDecl);
            return outerBuilder;
        }
    }
}