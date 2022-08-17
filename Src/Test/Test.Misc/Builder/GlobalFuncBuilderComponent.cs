using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using static Citron.Infra.Misc;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    internal class GlobalFuncBuilderComponent<TBuilder>
    {
        SymbolFactory factory;
        TBuilder builder;
        IHolder<ITopLevelDeclSymbolNode> outerHolder;
        ImmutableArray<GlobalFuncDeclSymbol>.Builder globalFuncDeclsBuilder;

        public GlobalFuncBuilderComponent(SymbolFactory factory, TBuilder builder, IHolder<ITopLevelDeclSymbolNode> outerHolder)
        {
            this.factory = factory;
            this.builder = builder;
            this.outerHolder = outerHolder;
            this.globalFuncDeclsBuilder = ImmutableArray.CreateBuilder<GlobalFuncDeclSymbol>();
        }

        // 인자 없음
        public TBuilder GlobalFunc(ITypeSymbol retType, string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
        {
            globalFuncDecl = new GlobalFuncDeclSymbol(
                outerHolder, AccessModifier.Public,
                new Holder<FuncReturn>(new FuncReturn(false, retType)), new Name.Normal(funcName), default,
                new Holder<ImmutableArray<FuncParameter>>(default),
                bInternal: true,
                lambdaDecls: default
            );

            globalFuncDeclsBuilder.Add(globalFuncDecl);
            return builder;
        }

        // 리턴 타입, 인자 1개짜리
        public TBuilder GlobalFunc(ITypeSymbol retType, string funcName, ITypeSymbol paramType, string paramName, out GlobalFuncDeclSymbol globalFuncDecl)
        {
            globalFuncDecl = new GlobalFuncDeclSymbol(
                outerHolder, AccessModifier.Public,
                new Holder<FuncReturn>(new FuncReturn(false, retType)), new Name.Normal(funcName), default,

                new Holder<ImmutableArray<FuncParameter>>(Arr<FuncParameter>(new FuncParameter(FuncParameterKind.Default, paramType, new Name.Normal(paramName)))),
                bInternal: true,
                lambdaDecls: default
            );

            globalFuncDeclsBuilder.Add(globalFuncDecl);
            return builder;
        }

        // 
        public TBuilder GlobalFunc(IHolder<FuncReturn> funcReturnHolder, string funcName,
            IHolder<ImmutableArray<FuncParameter>> funcParametersHolder, out GlobalFuncDeclSymbol globalFuncDecl)
        {
            globalFuncDecl = new GlobalFuncDeclSymbol(
                outerHolder, AccessModifier.Public,
                funcReturnHolder, new Name.Normal(funcName), default,
                funcParametersHolder,
                bInternal: true,
                lambdaDecls: default
            );

            globalFuncDeclsBuilder.Add(globalFuncDecl);
            return builder;
        }

        public GlobalFuncDeclBuilder<TBuilder> BeginGlobalFunc(IHolder<FuncReturn> funcRetHolder, Name funcName, IHolder<ImmutableArray<FuncParameter>> funcParamHolder)
        {
            return new GlobalFuncDeclBuilder<TBuilder>(factory, builder, lambdaDecls =>
            {
                var globalFuncDecl = new GlobalFuncDeclSymbol(outerHolder, AccessModifier.Public, funcRetHolder, funcName, default, funcParamHolder, true, lambdaDecls);
                globalFuncDeclsBuilder.Add(globalFuncDecl);

                return globalFuncDecl;
            });
        }        

        public ImmutableArray<GlobalFuncDeclSymbol> MakeGlobalFuncDecls()
        {
            return globalFuncDeclsBuilder.ToImmutable();
        }

        public bool HasTopLevel()
        {
            foreach (var decl in globalFuncDeclsBuilder.AsEnumerable())
            {
                var nodeName = decl.GetNodeName();
                if (nodeName.Name.Equals(Name.TopLevel))
                    return true;
            }

            return false;
        }
    }
}