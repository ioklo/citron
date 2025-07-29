#include "NTypeDeclContainerComponent.h"

#include <optional>

#include "RDecl.h"
#include "NTypeDecl.h"

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
    typeDict.insert_or_assign(typeDecl->GetNDecl()->GetRDecl()->GetIdentifier(), move(typeDecl));
}

// 첫번째 인자는 부모의 typeArgs
optional<RMember> NTypeDeclContainerComponent::GetMemberType(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto i = typeDict.find({ name, explicitTypeParamsExceptOuterCount, {} });
    if (i == typeDict.end()) return nullopt;

    return i->second->ToRMember(i->second, typeArgs);
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