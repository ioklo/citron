module Citron.DeclSymbols:TypeDeclSymbolComponent;
import <optional>;

using namespace std;

namespace Citron {

//TypeDeclSymbolComponent::TypeDeclSymbolComponent()
//{
//
//}

/*public IEnumerable<ITypeDeclSymbol> GetEnumerable()
{
    return typeDict.Values;
}*/

//TypeDeclSymbol* TypeDeclSymbolComponent::GetType(const Name& name)
//{
//    auto i = typeDict.find(name);
//    if (i == typeDict.end()) return nullptr;
//
//    return &types[i->second];
//}

void TypeDeclSymbolComponent::AddType(TypeDeclSymbol&& typeDecl)
{
    /*size_t index = types.size();

    auto& name = GetName(typeDecl);
    typeDict.insert(name, index);
    types.push_back(std::move(typeDecl));*/
}
//
//bool ICyclicEqualityComparableStruct<TypeDeclSymbolComponent>.CyclicEquals(ref TypeDeclSymbolComponent other, ref CyclicEqualityCompareContext context)
//{
//    if (!typeDict.CyclicEqualsClassValue(other.typeDict, ref context))
//        return false;
//
//    return true;
//}
//
//void ISerializable.DoSerialize(ref SerializeContext context)
//{
//    context.SerializeDictRefKeyRefValue(nameof(typeDict), typeDict);
//}



}