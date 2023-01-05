using System.Collections.Generic;
using Citron.Collections;
using Pretune;
using Citron.Module;
using Citron.Infra;

namespace Citron.Symbol
{   
    public struct TypeDeclSymbolComponent : ICyclicEqualityComparableStruct<TypeDeclSymbolComponent>
    {        
        Dictionary<DeclSymbolNodeName, ITypeDeclSymbol> typeDict;

        public static TypeDeclSymbolComponent Make()
        {
            var comp = new TypeDeclSymbolComponent();
            comp.typeDict = new Dictionary<DeclSymbolNodeName, ITypeDeclSymbol>();
            return comp;
        }
        
        public IEnumerable<ITypeDeclSymbol> GetEnumerable()
        {
            return typeDict.Values;
        }
        
        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
        {
            return typeDict.GetValueOrDefault(new DeclSymbolNodeName(name, typeParamCount, default));
        }

        public void AddType(ITypeDeclSymbol typeDecl)
        {
            typeDict.Add(typeDecl.GetNodeName(), typeDecl);
        }

        bool ICyclicEqualityComparableStruct<TypeDeclSymbolComponent>.CyclicEquals(ref TypeDeclSymbolComponent other, ref CyclicEqualityCompareContext context)
        {
            if (!typeDict.CyclicEqualsClassValue(other.typeDict, ref context))
                return false;

            return true;
        }
    }
}
