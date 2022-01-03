using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class TypeDict
    {
        public static TypeDict<TTypeDeclSymbol> Build<TTypeDeclSymbol>(ImmutableArray<TTypeDeclSymbol> types)
            where TTypeDeclSymbol : IDeclSymbolNode
        {
            var typesBuilder = ImmutableDictionary.CreateBuilder<DeclSymbolNodeName, TTypeDeclSymbol>();
            foreach (var type in types)
            {
                typesBuilder.Add(type.GetNodeName(), type);
            }

            var dict = new TypeDict<TTypeDeclSymbol>();
            dict.types = typesBuilder.ToImmutable();
            return dict;
        }
    }

    [ExcludeComparison]
    public partial struct TypeDict<TTypeDeclSymbol>
        where TTypeDeclSymbol : IDeclSymbolNode
    {
        internal ImmutableDictionary<DeclSymbolNodeName, TTypeDeclSymbol> types;
        
        public TTypeDeclSymbol? Get(DeclSymbolNodeName name)
        {
            return types.GetValueOrDefault(name);
        }
    }
}
