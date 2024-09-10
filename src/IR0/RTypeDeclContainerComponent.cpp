#include <optional>
#include "RTypeDeclContainerComponent.h"
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

RTypeDeclContainerComponent::RTypeDeclContainerComponent() = default;

size_t RTypeDeclContainerComponent::GetTypeCount()
{
    return types.size();
}

RTypeDeclPtr RTypeDeclContainerComponent::GetType(int index)
{
    return types[index];
}

RTypeDeclPtr RTypeDeclContainerComponent::GetType(const RIdentifier& identifier)
{
    auto i = typeDict.find(identifier);
    if (i == typeDict.end()) return nullptr;

    return i->second;
}

void RTypeDeclContainerComponent::AddType(RTypeDeclPtr typeDecl)
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