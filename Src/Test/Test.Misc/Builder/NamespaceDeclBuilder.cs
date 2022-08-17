using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    public class NamespaceDeclBuilder<TOuterBuilder>
    {
        // 람다 금지ㅋ
        internal delegate NamespaceDeclSymbol OnFinish(ImmutableArray<NamespaceDeclSymbol> namespaces, ImmutableArray<GlobalTypeDeclSymbol> types, ImmutableArray<GlobalFuncDeclSymbol> funcs);

        TOuterBuilder outerBuilder;
        OnFinish onFinish;

        Holder<NamespaceDeclSymbol> namespaceDeclHolder;

        // 자식 네임스페이스
        NamespaceBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>> namespaceComponent;
        TypeBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>, GlobalTypeDeclSymbol> globalTypeComponent;
        GlobalFuncBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>> globalFuncComponent;

        internal NamespaceDeclBuilder(SymbolFactory factory, TOuterBuilder outerBuilder, OnFinish onFinish)
        {
            this.outerBuilder = outerBuilder;
            this.onFinish = onFinish;

            this.namespaceDeclHolder = new Holder<NamespaceDeclSymbol>();
            this.namespaceComponent = new NamespaceBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>>(factory, this, namespaceDeclHolder);
            this.globalTypeComponent = new TypeBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>, GlobalTypeDeclSymbol>(this, typeDecl =>            
                new GlobalTypeDeclSymbol(namespaceDeclHolder, AccessModifier.Public, typeDecl)
            );            

            this.globalFuncComponent = new GlobalFuncBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>>(factory, this, namespaceDeclHolder);
        }

        public NamespaceDeclBuilder<NamespaceDeclBuilder<TOuterBuilder>> BeginNamespace(string name)
            => namespaceComponent.BeginNamespace(name);

        // redirects type component
        public ClassDeclBuilder<NamespaceDeclBuilder<TOuterBuilder>> BeginClass(string name)
            => globalTypeComponent.BeginClass(name);

        public NamespaceDeclBuilder<TOuterBuilder> Class(string name, ImmutableArray<string> typeParams, out ClassDeclSymbol decl)
            => globalTypeComponent.Class(name, typeParams, out decl);

        public NamespaceDeclBuilder<TOuterBuilder> Struct(string name, out StructDeclSymbol decl)
            => globalTypeComponent.Struct(name, out decl);        

        public NamespaceDeclBuilder<TOuterBuilder> GlobalFunc(ITypeSymbol retType, string funcName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(retType, funcName, out globalFuncDecl);

        public NamespaceDeclBuilder<TOuterBuilder> GlobalFunc(ITypeSymbol retType, string funcName, ITypeSymbol paramType, string paramName, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(retType, funcName, paramType, paramName, out globalFuncDecl);

        public NamespaceDeclBuilder<TOuterBuilder> GlobalFunc(IHolder<FuncReturn> funcReturnHolder, string funcName,
            IHolder<ImmutableArray<FuncParameter>> funcParametersHolder, out GlobalFuncDeclSymbol globalFuncDecl)
            => globalFuncComponent.GlobalFunc(funcReturnHolder, funcName, funcParametersHolder, out globalFuncDecl);        

        public GlobalFuncDeclBuilder<NamespaceDeclBuilder<TOuterBuilder>> BeginGlobalFunc(IHolder<FuncReturn> funcRetHolder, Name funcName, IHolder<ImmutableArray<FuncParameter>> funcParamHolder)
            => globalFuncComponent.BeginGlobalFunc(funcRetHolder, funcName, funcParamHolder);

        

        // NOTICE: not relevant BeginNamespace above
        public TOuterBuilder EndNamespace(out NamespaceDeclSymbol namespaceDecl)
        {
            namespaceDecl = onFinish.Invoke(namespaceComponent.MakeNamespaceDecls(), globalTypeComponent.MakeTypeDecls(), globalFuncComponent.MakeGlobalFuncDecls());
            namespaceDeclHolder.SetValue(namespaceDecl);
            return outerBuilder;
        }
    }
}