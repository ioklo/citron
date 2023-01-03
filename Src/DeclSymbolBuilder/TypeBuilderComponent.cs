using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using Citron.Symbol;

using static Citron.Infra.Misc;

namespace Citron.Test
{
    internal class TypeBuilderComponent<TBuilder>
    {   
        TBuilder builder;
        SymbolFactory factory;
        IDeclSymbolNode outer;
        Accessor defaultAccessModifier;
        ImmutableArray<ITypeDeclSymbol>.Builder typeDeclSymbols;

        public TypeBuilderComponent(TBuilder builder, SymbolFactory factory, IDeclSymbolNode outer, Accessor defaultAccessModifier)
        {
            this.builder = builder;
            this.factory = factory;
            this.outer = outer;
            this.defaultAccessModifier = defaultAccessModifier;
            this.typeDeclSymbols = ImmutableArray.CreateBuilder<ITypeDeclSymbol>();
        }

        public ClassDeclBuilder<TBuilder> BeginClass(string name)
        {
            return new ClassDeclBuilder<TBuilder>(builder, (typeParams, memberFuncs) =>
            {
                var typeDecl = new ClassDeclSymbol(
                    outer,
                    defaultAccessModifier,
                    new Name.Normal(name),
                    typeParams,
                    baseClassHolder: new Holder<ClassSymbol?>(null),
                    interfacesHolder: new Holder<ImmutableArray<InterfaceSymbol>>(),
                    memberTypesExceptTypeVars: default,
                    memberFuncs,
                    memberVars: default,
                    constructorDeclsHolder: new Holder<ImmutableArray<ClassConstructorDeclSymbol>>(default),
                    trivialConstructorHolder: new Holder<ClassConstructorDeclSymbol>(null)
                );

                typeDeclSymbols.Add(typeDecl);
                return typeDecl;
            });
        }

        public TBuilder Class(string name, out ClassDeclSymbol classDecl)
        {
            classDecl = new ClassDeclSymbol(
                outerHolder,
                defaultAccessModifier,
                new Name.Normal(name),
                typeParams: default, new Holder<ClassSymbol>(null), Arr<InterfaceSymbol>().ToHolder(),
                memberTypesExceptTypeVars: default,
                memberFuncs: default,
                memberVars: default,                
                Arr<ClassConstructorDeclSymbol>().ToHolder(),
                default(ClassConstructorDeclSymbol).ToHolder()
            );

            typeDeclSymbols.Add(classDecl);
            return builder;
        }

        // 빈 깡통
        public TBuilder Struct(string name, out StructDeclSymbol structDecl)
        {
            structDecl = new StructDeclSymbol(
                outerHolder,
                defaultAccessModifier, 
                new Name.Normal(name), default, new Holder<StructSymbol?>(null),
                memberTypesExceptTypeVars: default,
                memberFuncs: default,
                memberVars: default,
                new Holder<ImmutableArray<StructConstructorDeclSymbol>>(default),
                new Holder<StructConstructorDeclSymbol?>(null)
            );

            typeDeclSymbols.Add(structDecl);
            return builder;
        }
        
        public StructDeclBuilder<TBuilder> BeginStruct(Accessor accessModifier, string name)
        {
            return new StructDeclBuilder<TBuilder>(builder, factory, typeDeclSymbols, outerHolder, accessModifier, name);
        }

        public ImmutableArray<ITypeDeclSymbol> MakeTypeDecls()
        {
            return typeDeclSymbols.ToImmutable();
        }
    }
}