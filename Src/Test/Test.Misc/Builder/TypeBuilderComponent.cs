using Citron.Analysis;
using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;

namespace Citron.Test.Misc
{
    internal class TypeBuilderComponent<TBuilder, TTypeDeclSymbolContainer>
        where TTypeDeclSymbolContainer : ITypeDeclSymbolContainer
    {
        internal delegate TTypeDeclSymbolContainer OnFinish(ITypeDeclSymbol typeDecl);
        
        TBuilder builder;
        OnFinish onFinish;

        public TypeBuilderComponent(TBuilder builder, OnFinish onFinish)
        {
            this.builder = builder;
            this.onFinish = onFinish;
        }

        public ClassDeclBuilder<TBuilder> BeginClass(string name)
        {
            return new ClassDeclBuilder<TBuilder>(builder, memberFuncs =>
            {
                var containerHolder = new Holder<ITypeDeclSymbolContainer>();

                var typeDecl = new ClassDeclSymbol(
                    containerHolder,
                    new Name.Normal(name),
                    typeParams: default,
                    baseClassHolder: new Holder<ClassSymbol?>(null),
                    interfacesHolder: new Holder<ImmutableArray<InterfaceSymbol>>(),
                    memberTypes: default,
                    memberFuncs,
                    memberVars: default,
                    constructorDeclsHolder: new Holder<ImmutableArray<ClassConstructorDeclSymbol>>(default),
                    trivialConstructorHolder: new Holder<ClassConstructorDeclSymbol>(null)
                );

                var container = onFinish.Invoke(typeDecl);
                containerHolder.SetValue(container);

                return typeDecl;
            });
        }
    }
}