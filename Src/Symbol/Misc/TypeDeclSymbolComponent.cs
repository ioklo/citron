using System.Collections.Generic;
using Citron.Collections;
using Pretune;
using Citron.Module;

namespace Citron.Symbol
{   
    [ExcludeComparison]
    public partial struct TypeDeclSymbolComponent
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
    }
}
