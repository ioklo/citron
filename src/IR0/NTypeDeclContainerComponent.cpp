#include <optional>
#include "NTypeDeclContainerComponent.h"
#include "RNames.h"

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

NTypeDeclContainerComponent::NTypeDeclContainerComponent() = default;

size_t NTypeDeclContainerComponent::GetTypeCount()
{
    return types.size();
}

NTypeDeclPtr NTypeDeclContainerComponent::GetType(int index)
{
    return types[index];
}

NTypeDeclPtr NTypeDeclContainerComponent::GetType(const RIdentifier& identifier)
{
    auto i = typeDict.find(identifier);
    if (i == typeDict.end()) return nullptr;

    return i->second;
}

void NTypeDeclContainerComponent::AddType(NTypeDeclPtr&& typeDecl)
{
    types.push_back(typeDecl);
    typeDict.insert_or_assign(typeDecl->GetIdentifier(), std::move(typeDecl));
}

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