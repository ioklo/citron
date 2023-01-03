using Citron.Collections;
using Citron.Infra;
using Citron.Module;
using Citron.Symbol;
using System.Diagnostics;

namespace Citron.Test
{
    public class GlobalFuncDeclBuilder<TOuterBuilder, TOuterDeclSymbol>
        where TOuterDeclSymbol : ITopLevelDeclSymbolNode, ITopLevelDeclContainable
    {   
        TOuterBuilder outerBuilder;
        ImmutableArray<GlobalFuncDeclSymbol>.Builder globalFuncDeclsBuilder; // result

        TOuterDeclSymbol outer;
        Accessor accessModifier;
        Name funcName;
        bool bInternal;

        GlobalFuncDeclSymbol declSymbol;        
        
        FuncRetParamsBuilderComponent<GlobalFuncDeclBuilder<TOuterBuilder, TOuterDeclSymbol>> funcRetParamsComponent;
        LambdaBuilderComponent<GlobalFuncDeclBuilder<TOuterBuilder, TOuterDeclSymbol>> lambdaComponent;

        internal GlobalFuncDeclBuilder(
            SymbolFactory factory, 
            TOuterBuilder outerBuilder,
            ImmutableArray<GlobalFuncDeclSymbol>.Builder globalFuncDeclsBuilder,
            TOuterDeclSymbol outer,
            Accessor accessModifier,
            Name funcName,
            ImmutableArray<Name> typeParams,
            bool bInternal)
        {
            this.outerBuilder = outerBuilder;
            this.globalFuncDeclsBuilder = globalFuncDeclsBuilder;

            this.outer = outer;
            this.accessModifier = accessModifier;
            this.funcName = funcName;
            this.bInternal = bInternal;

            this.declSymbol = new GlobalFuncDeclSymbol(outer, accessModifier, funcName, typeParams, bInternal);
            this.funcRetParamsComponent = new FuncRetParamsBuilderComponent<GlobalFuncDeclBuilder<TOuterBuilder, TOuterDeclSymbol>>(this);
            this.lambdaComponent = new LambdaBuilderComponent<GlobalFuncDeclBuilder<TOuterBuilder, TOuterDeclSymbol>>(factory, this, declSymbol);
        }
        
        public GlobalFuncDeclBuilder<TOuterBuilder> TypeParam(string name, out Name typeVarDecl)
            => typeParamComponent.TypeParam(name, out typeVarDecl);

        public GlobalFuncDeclBuilder<TOuterBuilder> Lambda(out LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.Lambda(out lambdaDecl);

        public GlobalFuncDeclBuilder<TOuterBuilder> Lambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams, out LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.Lambda(funcRet, funcParams, out lambdaDecl);

        public LambdaDeclBuilder<GlobalFuncDeclBuilder<TOuterBuilder>> BeginLambda()
            => lambdaComponent.BeginLambda();

        public LambdaDeclBuilder<GlobalFuncDeclBuilder<TOuterBuilder>> BeginLambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams)
            => lambdaComponent.BeginLambda(funcRet, funcParams);

        // embed FuncRetParamsBuilderComponent
        public GlobalFuncDeclBuilder<TOuterBuilder> FuncReturn(bool isRef, ITypeSymbol type)
            => funcRetParamsComponent.FuncReturn(isRef, type);

        public GlobalFuncDeclBuilder<TOuterBuilder> FuncReturnHolder(out Holder<FuncReturn> funcRetHolder)
            => funcRetParamsComponent.FuncReturnHolder(out funcRetHolder);

        public GlobalFuncDeclBuilder<TOuterBuilder> FuncParameter(FuncParameterKind kind, ITypeSymbol type, Name name)
            => funcRetParamsComponent.FuncParameter(kind, type, name);

        public GlobalFuncDeclBuilder<TOuterBuilder> FuncParametersHolder(ImmutableArray<FuncParamId> paramIds, out Holder<ImmutableArray<FuncParameter>> funcParamsHolder)
            => funcRetParamsComponent.FuncParametersHolder(paramIds, out funcParamsHolder);

        // sugar
        public TOuterBuilder EndTopLevelFunc(out GlobalFuncDeclSymbol topLevelFuncDecl)
            => EndGlobalFunc(out topLevelFuncDecl);        

        public TOuterBuilder EndGlobalFunc(out GlobalFuncDeclSymbol globalFuncDecl)
        {
            var (funcRetHolder, funcParamsHolder) = funcRetParamsComponent.Get();

            var typeParams = typeParamComponent.MakeTypeParams();
            var lambdaDecls = lambdaComponent.MakeLambdaDecls();

            globalFuncDecl = new GlobalFuncDeclSymbol(
                outer, accessModifier, funcRetHolder,
                funcName, typeParams, funcParamsHolder, bInternal, lambdaDecls);
            
            globalFuncDeclHolder.SetValue(globalFuncDecl);
            globalFuncDeclsBuilder.Add(globalFuncDecl);
            return outerBuilder;
        }
    }
}