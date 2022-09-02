using System.Collections.Generic;
using Citron.Collections;
using Pretune;
using Citron.Module;

namespace Citron.Symbol
{   
    [ExcludeComparison]
    public partial struct TypeDict
    {
        public static TypeDict Build(ImmutableArray<TypeVarDeclSymbol> typeVars, ImmutableArray<ITypeDeclSymbol> types)
        {
            var typesBuilder = ImmutableDictionary.CreateBuilder<DeclSymbolNodeName, ITypeDeclSymbol>();
            foreach(var typeVar in typeVars)
                typesBuilder.Add(typeVar.GetNodeName(), typeVar);

            foreach (var type in types)
                typesBuilder.Add(type.GetNodeName(), type);

            var dict = new TypeDict();
            dict.types = typesBuilder.ToImmutable();
            return dict;
        }

        internal ImmutableDictionary<DeclSymbolNodeName, ITypeDeclSymbol> types;

        public IEnumerable<ITypeDeclSymbol> GetEnumerable()
        {
            return types.Values;
        }
        
        public ITypeDeclSymbol? Get(DeclSymbolNodeName name)
        {
            return types.GetValueOrDefault(name);
        }
    }
}
