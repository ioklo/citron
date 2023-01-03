using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using Citron.Symbol;

namespace Citron.Test
{
    internal class NamespaceBuilderComponent<TBuilder>
    {
        SymbolFactory factory;
        TBuilder builder;
        ITopLevelDeclSymbolNode outer;

        ImmutableArray<NamespaceDeclSymbol>.Builder namespaceDeclsBuilder;

        public NamespaceBuilderComponent(SymbolFactory factory, TBuilder builder, ITopLevelDeclSymbolNode outer)
        {
            this.factory = factory;
            this.builder = builder;
            this.outer = outer;
            this.namespaceDeclsBuilder = ImmutableArray.CreateBuilder<NamespaceDeclSymbol>();
        }

        public NamespaceDeclBuilder<TBuilder> BeginNamespace(string name)
        {
            var ns = new NamespaceDeclSymbol(outer, new Name.Normal(name));

            return new NamespaceDeclBuilder<TBuilder>(factory, builder, (namespaces, types, funcs) =>
            {
                var ns = new NamespaceDeclSymbol(outer, new Name.Normal(name), namespaces, types, funcs);
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