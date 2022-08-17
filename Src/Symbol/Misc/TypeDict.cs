using System.Collections.Generic;
using Citron.Collections;
using Pretune;
using Citron.Module;

namespace Citron.Symbol
{
    public class TypeDict
    {
        public interface IHaveNodeName
        {
            DeclSymbolNodeName GetNodeName();
        }

        public static TypeDict<TTypeDeclSymbol> Build<TTypeDeclSymbol>(ImmutableArray<TTypeDeclSymbol> types)
            where TTypeDeclSymbol : IHaveNodeName
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
        where TTypeDeclSymbol : TypeDict.IHaveNodeName
    {
        internal ImmutableDictionary<DeclSymbolNodeName, TTypeDeclSymbol> types;

        public IEnumerable<TTypeDeclSymbol> GetEnumerable()
        {
            return types.Values;
        }
        
        public TTypeDeclSymbol? Get(DeclSymbolNodeName name)
        {
            return types.GetValueOrDefault(name);
        }
    }
}
