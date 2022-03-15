using Citron.Analysis;
using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;
using System;

namespace Citron.Test.Misc
{
    internal class LambdaBuilderComponent<TBuilder>
    {
        SymbolFactory factory;

        TBuilder builder;
        IHolder<IFuncDeclSymbol> lambdaDeclContainerHolder;

        ImmutableArray<LambdaDeclSymbol>.Builder lambdaDeclsBuilder;
        int anonymousCount; // TODO: lambda에만 있으므로 이름을 바꾸거나 나중에 AnonymousCountComponent를 만들던가

        public LambdaBuilderComponent(SymbolFactory factory, TBuilder builder, IHolder<IFuncDeclSymbol> lambdaDeclContainerHolder)
        {
            this.factory = factory;

            this.builder = builder;
            this.lambdaDeclContainerHolder = lambdaDeclContainerHolder;

            this.lambdaDeclsBuilder = ImmutableArray.CreateBuilder<LambdaDeclSymbol>();
            this.anonymousCount = 0;
        }

        public ImmutableArray<LambdaDeclSymbol> MakeLambdaDecls()
        {
            return lambdaDeclsBuilder.ToImmutable();
        }

        // without members
        public TBuilder Lambda(out LambdaDeclSymbol lambdaDecl)
        {
            lambdaDecl = new LambdaDeclSymbol(
                lambdaDeclContainerHolder, new Name.Anonymous(anonymousCount),
                new FuncReturn(false, factory.MakeVoid()), parameters: default, memberVars: default, lambdaDecls: default);
            lambdaDeclsBuilder.Add(lambdaDecl);

            anonymousCount++;
            return builder;
        }

        // without members
        public TBuilder Lambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams, out LambdaDeclSymbol lambdaDecl)
        {
            lambdaDecl = new LambdaDeclSymbol(
                lambdaDeclContainerHolder, new Name.Anonymous(anonymousCount),
                funcRet, funcParams, memberVars: default, lambdaDecls: default);
            lambdaDeclsBuilder.Add(lambdaDecl);

            anonymousCount++;
            return builder;
        }

        public LambdaDeclBuilder<TBuilder> BeginLambda()
        {
            int curCount = anonymousCount; // holder, 지우지 말 것
            var lambdaDeclBuilder = new LambdaDeclBuilder<TBuilder>(builder, memberVarDecls =>
            {
                var lambdaDecl = new LambdaDeclSymbol(lambdaDeclContainerHolder, new Name.Anonymous(curCount),
                    new FuncReturn(false, factory.MakeVoid()), default, memberVarDecls, lambdaDecls: default);

                lambdaDeclsBuilder.Add(lambdaDecl);
                return lambdaDecl;
            });

            anonymousCount++;
            return lambdaDeclBuilder;
        }

        public LambdaDeclBuilder<TBuilder> BeginLambda(FuncReturn funcRet, ImmutableArray<FuncParameter> funcParams)
        {
            int curCount = anonymousCount; // holder, 지우지 말 것
            anonymousCount++;

            return new LambdaDeclBuilder<TBuilder>(builder, memberVarDecls =>
            {
                var lambdaDecl = new LambdaDeclSymbol(lambdaDeclContainerHolder, new Name.Anonymous(curCount), funcRet, funcParams, memberVarDecls, lambdaDecls: default);
                lambdaDeclsBuilder.Add(lambdaDecl);
                return lambdaDecl;
            });
        }
    }
}