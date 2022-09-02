using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    internal class TypeParamBuilderComponent<TBuilder>
    {
        TBuilder builder;
        IHolder<IDeclSymbolNode> outerHolder;
        ImmutableArray<TypeVarDeclSymbol>.Builder typeParamsBuilder;

        public TypeParamBuilderComponent(TBuilder builder, IHolder<IDeclSymbolNode> outerHolder)
        {
            this.builder = builder;
            this.outerHolder = outerHolder;
            this.typeParamsBuilder = ImmutableArray.CreateBuilder<TypeVarDeclSymbol>();
        }

        public TBuilder TypeParam(string name, out TypeVarDeclSymbol typeVarDecl)
        {
            typeVarDecl = new TypeVarDeclSymbol(outerHolder, new Name.Normal(name));
            typeParamsBuilder.Add(typeVarDecl);
            return builder;
        }

        public ImmutableArray<TypeVarDeclSymbol> MakeTypeParams()
        {
            return typeParamsBuilder.ToImmutable();
        }
    }
}