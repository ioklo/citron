using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using Citron.Symbol;

namespace Citron.Test
{
    public class NamespaceDeclBuilder<TOuterBuilder>
    {
        // 람다 금지ㅋ
        internal delegate NamespaceDeclSymbol OnFinish(ImmutableArray<NamespaceDeclSymbol> namespaces, ImmutableArray<ITypeDeclSymbol> types, ImmutableArray<GlobalFuncDeclSymbol> funcs);

        TOuterBuilder outerBuilder;
        OnFinish onFinish;

        NamespaceDeclSymbol namespaceDecl;

        // 자식 네임스페이스
        NamespaceBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>> namespaceComponent;
        TypeBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>> globalTypeComponent;
        GlobalFuncBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>, NamespaceDeclSymbol> globalFuncComponent;

        internal NamespaceDeclBuilder(SymbolFactory factory, TOuterBuilder outerBuilder, OnFinish onFinish)
        {
            this.outerBuilder = outerBuilder;
            this.onFinish = onFinish;

            this.namespaceComponent = new NamespaceBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>>(factory, this, namespaceDecl);
            this.globalTypeComponent = new TypeBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>>(this, factory, namespaceDecl, Accessor.Private);
            this.globalFuncComponent = new GlobalFuncBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>, NamespaceDeclSymbol>(this, namespaceDecl);
        }

        public NamespaceDeclBuilder<NamespaceDeclBuilder<TOuterBuilder>> BeginNamespace(string name)
            => namespaceComponent.BeginNamespace(name);

        // redirects type component
        public ClassDeclBuilder<NamespaceDeclBuilder<TOuterBuilder>> BeginClass(string name)
            => globalTypeComponent.BeginClass(name);

        public NamespaceDeclBuilder<TOuterBuilder> Class(string name, out ClassDeclSymbol decl)
            => globalTypeComponent.Class(name, out decl);

        public NamespaceDeclBuilder<TOuterBuilder> Struct(string name, out StructDeclSymbol decl)
            => globalTypeComponent.Struct(name, out decl);

        public NamespaceDeclBuilder<TOuterBuilder> GlobalFunc(Accessor accessor, FuncReturn funcReturn, string funcName, ImmutableArray<Name> typeParams, ImmutableArray<FuncParameter> funcParams)
            => globalFuncComponent.GlobalFunc(accessor, funcReturn, funcName, typeParams, funcParams);

        public NamespaceDeclBuilder<TOuterBuilder> GlobalFunc(Accessor accessor, string funcName, ImmutableArray<Name> typeParams, GlobalFuncBuilderComponent<NamespaceDeclBuilder<TOuterBuilder>, NamespaceDeclSymbol>.PostSkeletonPhaseTask task)
            => globalFuncComponent.GlobalFunc(accessor, funcName, typeParams, task);

        // NOTICE: not relevant BeginNamespace above
        public TOuterBuilder EndNamespace(out NamespaceDeclSymbol namespaceDecl)
        {
            namespaceDecl = this.namespaceDecl;

            namespaceDecl = onFinish.Invoke(namespaceComponent.MakeNamespaceDecls(), globalTypeComponent.MakeTypeDecls(), globalFuncComponent.MakeGlobalFuncDecls());
            namespaceDecl.SetValue(namespaceDecl);
            return outerBuilder;
        }
    }
}