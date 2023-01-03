using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using static Citron.Infra.Misc;
using Citron.Symbol;

namespace Citron.Test
{
    internal class GlobalFuncBuilderComponent<TBuilder, TOuterDeclSymbol>
        where TOuterDeclSymbol : ITopLevelDeclSymbolNode, ITopLevelDeclContainable
    {
        SymbolFactory factory;
        TBuilder builder;
        TOuterDeclSymbol outer;

        public GlobalFuncBuilderComponent(SymbolFactory factory, TBuilder builder, TOuterDeclSymbol outer)
        {
            this.factory = factory;
            this.builder = builder;
            this.outer = outer;
        }

        // 모든 인자
        public TBuilder GlobalFunc(FuncReturn funcReturn, string funcName, ImmutableArray<FuncParameter> funcParams, out GlobalFuncDeclSymbol globalFuncDecl)
        {
            globalFuncDecl = new GlobalFuncDeclSymbol(
                outer, Accessor.Public, new Name.Normal(funcName), typeParams: default, bInternal: true
            );

            outer.AddFunc(globalFuncDecl);

            globalFuncDecl.InitFuncReturnAndParams(funcReturn, funcParams);

            return builder;
        }

        // 인자 없음
        public TBuilder GlobalFunc(ITypeSymbol retType, string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
        {
            globalFuncDecl = new GlobalFuncDeclSymbol(
                outer, Accessor.Public, new Name.Normal(funcName), typeParams: default, bInternal: true
            );
            outer.AddFunc(globalFuncDecl);

            globalFuncDecl.InitFuncReturnAndParams(new FuncReturn(false, retType), parameters: default);
            
            return builder;
        }

        // 리턴 타입, 인자 1개짜리
        public TBuilder GlobalFunc(ITypeSymbol retType, string funcName, ITypeSymbol paramType, string paramName, out GlobalFuncDeclSymbol globalFuncDecl)
        {
            globalFuncDecl = new GlobalFuncDeclSymbol(
                outer, Accessor.Public,
                new Name.Normal(funcName), 
                typeParams: default,
                bInternal: true
            );
            outer.AddFunc(globalFuncDecl);

            globalFuncDecl.InitFuncReturnAndParams(
                new FuncReturn(false, retType),
                Arr<FuncParameter>(new FuncParameter(FuncParameterKind.Default, paramType, new Name.Normal(paramName)))
            );

            return builder;
        }

        // 인자를 추후에 설정하는
        // InitFuncReturnAndParams가 안불린 상태로 리턴한다
        public TBuilder GlobalFunc(string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
        {
            globalFuncDecl = new GlobalFuncDeclSymbol(
                outer, Accessor.Public,
                new Name.Normal(funcName), 
                typeParams: default, 
                bInternal: true
            );

            outer.AddFunc(globalFuncDecl);
            return builder;
        }
        
        public GlobalFuncDeclBuilder<TBuilder> BeginGlobalFunc(Accessor accessModifier, Name funcName, bool bInternal)
        {
            return new GlobalFuncDeclBuilder<TBuilder>(factory, builder, outer, accessModifier, funcName, bInternal);

            //{
            //    var globalFuncDecl = new GlobalFuncDeclSymbol(outerHolder, accessModifier, returnHolder, funcName, typeParams, parametersHolder, bInternal, lambdaDecls);
            //    globalFuncDeclsBuilder.Add(globalFuncDecl);
            //    return globalFuncDecl;
            //});
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