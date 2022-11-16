using Citron.Collections;
using Citron.Infra;
using Citron.Module;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    public class StructMemberFuncDeclBuilder<TOuterBuilder>
    {
        TOuterBuilder outerBuilder;
        ImmutableArray<StructMemberFuncDeclSymbol>.Builder funcDeclsBuilder;
        IHolder<StructDeclSymbol> outerHolder;
        AccessModifier accessModifier;
        bool bStatic;
        Name name;

        Holder<StructMemberFuncDeclSymbol> funcDeclHolder;
        TypeParamBuilderComponent<StructMemberFuncDeclBuilder<TOuterBuilder>> typeParamComponent;
        FuncRetParamsBuilderComponent<StructMemberFuncDeclBuilder<TOuterBuilder>> funcRetParamsComponent;
        LambdaBuilderComponent<StructMemberFuncDeclBuilder<TOuterBuilder>> lambdaComponent;

        internal StructMemberFuncDeclBuilder(
            TOuterBuilder outerBuilder,
            ImmutableArray<StructMemberFuncDeclSymbol>.Builder funcDeclsBuilder,
            SymbolFactory factory, IHolder<StructDeclSymbol> outerHolder, AccessModifier accessModifier, bool bStatic, Name name)
        {
            this.outerBuilder = outerBuilder;
            this.funcDeclsBuilder = funcDeclsBuilder;
            this.outerHolder = outerHolder;
            this.accessModifier = accessModifier;
            this.bStatic = bStatic;
            this.name = name;

            this.funcDeclHolder = new Holder<StructMemberFuncDeclSymbol>();
            this.typeParamComponent = new TypeParamBuilderComponent<StructMemberFuncDeclBuilder<TOuterBuilder>>(this, funcDeclHolder);
            this.funcRetParamsComponent = new FuncRetParamsBuilderComponent<StructMemberFuncDeclBuilder<TOuterBuilder>>(this);
            this.lambdaComponent = new LambdaBuilderComponent<StructMemberFuncDeclBuilder<TOuterBuilder>>(factory, this, funcDeclHolder);
        }


        public StructMemberFuncDeclBuilder<TOuterBuilder> TypeParam(string name, out TypeVarDeclSymbol typeVarDecl)
            => typeParamComponent.TypeParam(name, out typeVarDecl);

        public StructMemberFuncDeclBuilder<TOuterBuilder> Lambda(out LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.Lambda(out lambdaDecl);

        public StructMemberFuncDeclBuilder<TOuterBuilder> Lambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams, out LambdaDeclSymbol lambdaDecl)
            => lambdaComponent.Lambda(funcRet, funcParams, out lambdaDecl);

        public LambdaDeclBuilder<StructMemberFuncDeclBuilder<TOuterBuilder>> BeginLambda()
            => lambdaComponent.BeginLambda();

        public LambdaDeclBuilder<StructMemberFuncDeclBuilder<TOuterBuilder>> BeginLambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams)
            => lambdaComponent.BeginLambda(funcRet, funcParams);

        // embed FuncRetParamsBuilderComponent
        public StructMemberFuncDeclBuilder<TOuterBuilder> FuncReturn(bool isRef, ITypeSymbol type)
            => funcRetParamsComponent.FuncReturn(isRef, type);

        public StructMemberFuncDeclBuilder<TOuterBuilder> FuncReturnHolder(out Holder<FuncReturn> funcRetHolder)
            => funcRetParamsComponent.FuncReturnHolder(out funcRetHolder);

        public StructMemberFuncDeclBuilder<TOuterBuilder> FuncParameter(FuncParameterKind kind, ITypeSymbol type, Name name)
            => funcRetParamsComponent.FuncParameter(kind, type, name);

        public StructMemberFuncDeclBuilder<TOuterBuilder> FuncParametersHolder(ImmutableArray<FuncParamId> paramIds, out Holder<ImmutableArray<FuncParameter>> funcParamsHolder)
            => funcRetParamsComponent.FuncParametersHolder(paramIds, out funcParamsHolder);

        public TOuterBuilder EndFunc(out StructMemberFuncDeclSymbol funcDecl)
        {
            var typeParams = typeParamComponent.MakeTypeParams();
            var (retHolder, paramIds, paramsHolder) = funcRetParamsComponent.Get();
            var lambdaDecls = lambdaComponent.MakeLambdaDecls();

            funcDecl = new StructMemberFuncDeclSymbol(outerHolder, accessModifier, bStatic, retHolder, name, typeParams, paramsHolder, lambdaDecls);
            funcDeclHolder.SetValue(funcDecl);
            funcDeclsBuilder.Add(funcDecl);

            return outerBuilder;
        }
    }
}