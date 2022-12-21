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
        ImmutableArray<Name>.Builder typeParamsBuilder;

        public TypeParamBuilderComponent(TBuilder builder, IHolder<IDeclSymbolNode> outerHolder)
        {
            this.builder = builder;
            this.outerHolder = outerHolder;
            this.typeParamsBuilder = ImmutableArray.CreateBuilder<Name>();
        }

        public TBuilder TypeParam(string name, out Name typeVarDecl)
        {
            typeVarDecl = new Name.Normal(name);
            typeParamsBuilder.Add(typeVarDecl);
            return builder;
        }

        public ImmutableArray<Name> MakeTypeParams()
        {
            return typeParamsBuilder.ToImmutable();
        }
    }
}