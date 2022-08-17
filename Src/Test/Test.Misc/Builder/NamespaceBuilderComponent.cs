using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    internal class NamespaceBuilderComponent<TBuilder>
    {
        SymbolFactory factory;
        TBuilder builder;
        IHolder<ITopLevelDeclSymbolNode> outerHolder;

        ImmutableArray<NamespaceDeclSymbol>.Builder namespaceDeclsBuilder;

        public NamespaceBuilderComponent(SymbolFactory factory, TBuilder builder, IHolder<ITopLevelDeclSymbolNode> outerHolder)
        {
            this.factory = factory;
            this.builder = builder;
            this.outerHolder = outerHolder;
            this.namespaceDeclsBuilder = ImmutableArray.CreateBuilder<NamespaceDeclSymbol>();
        }

        public NamespaceDeclBuilder<TBuilder> BeginNamespace(string name)
        {
            return new NamespaceDeclBuilder<TBuilder>(factory, builder, (namespaces, types, funcs) =>
            {
                var ns = new NamespaceDeclSymbol(outerHolder, new Name.Normal(name), namespaces, types, funcs);
                namespaceDeclsBuilder.Add(ns);

                return ns;
            });
        }

        public ImmutableArray<NamespaceDeclSymbol> MakeNamespaceDecls()
        {
            return namespaceDeclsBuilder.ToImmutable();
        }
    }
}