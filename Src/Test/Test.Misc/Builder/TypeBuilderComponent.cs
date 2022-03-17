using Citron.Analysis;
using Citron.Collections;
using Citron.CompileTime;
using Citron.Infra;

namespace Citron.Test.Misc
{
    internal class TypeBuilderComponent<TBuilder, TTypeDeclSymbolContainer>
        where TTypeDeclSymbolContainer : ITypeDeclSymbolContainer
    {
        internal delegate TTypeDeclSymbolContainer MakeContainer(ITypeDeclSymbol typeDecl);
        
        TBuilder builder;
        MakeContainer makeContainerFunc;

        ImmutableArray<TTypeDeclSymbolContainer>.Builder containers;

        public TypeBuilderComponent(TBuilder builder, MakeContainer makeContainerFunc)
        {
            this.builder = builder;
            this.makeContainerFunc = makeContainerFunc;

            this.containers = ImmutableArray.CreateBuilder<TTypeDeclSymbolContainer>();
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

                var container = makeContainerFunc.Invoke(typeDecl);
                containerHolder.SetValue(container);

                containers.Add(container);

                return typeDecl;
            });
        }
        
        public TBuilder Class(string name, ImmutableArray<string> typeParams, out ClassDeclSymbol classDecl)
        {
            var containerHolder = new Holder<ITypeDeclSymbolContainer>();

            classDecl = new ClassDeclSymbol(containerHolder, new Name.Normal(name),
                typeParams,
                new Holder<ClassSymbol?>(null),
                new Holder<ImmutableArray<InterfaceSymbol>>(default),
                memberTypes: default,
                memberFuncs: default,
                memberVars: default,
                new Holder<ImmutableArray<ClassConstructorDeclSymbol>>(default),
                new Holder<ClassConstructorDeclSymbol?>(null)
            );

            var container = makeContainerFunc.Invoke(classDecl);
            containerHolder.SetValue(container);

            containers.Add(container);

            return builder;
        }

        // 빈 깡통
        public TBuilder Struct(string name, out StructDeclSymbol structDecl)
        {
            var containerHolder = new Holder<ITypeDeclSymbolContainer>();

            structDecl = new StructDeclSymbol(containerHolder, new Name.Normal(name), default, new Holder<StructSymbol?>(null),
                typeDecls: default,
                funcDecls: default,
                memberVars: default,
                new Holder<ImmutableArray<StructConstructorDeclSymbol>>(default),
                new Holder<StructConstructorDeclSymbol?>(null)
            );

            var container = makeContainerFunc.Invoke(structDecl);
            containerHolder.SetValue(container);

            containers.Add(container);

            return builder;
        }

        public ImmutableArray<TTypeDeclSymbolContainer> MakeTypeDecls()
        {
            return containers.ToImmutable();
        }
    }
}