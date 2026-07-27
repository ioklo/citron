#include "RTypeDeclContainerComponent.h"
#include <optional>
#include "RDecl.h"

using namespace std;

namespace Citron {

RTypeDeclContainerComponent::RTypeDeclContainerComponent() = default;

size_t RTypeDeclContainerComponent::GetTypeCount()
{
    return types.size();
}

RTypeDecl* RTypeDeclContainerComponent::GetType(int index)
{
    if (index < 0 || index >= types.size()) return nullptr;
    return types[index];
}

void RTypeDeclContainerComponent::AddType(RTypeDecl* typeDecl)
{
    types.push_back(typeDecl);

    auto* name = typeDecl->RTypeDecl_GetDecl()->TryGetName();
    assert(name); // typeDecl은 항상 이름이 있다

    typeDict.insert_or_assign(*name, typeDecl);
}

RTypeDecl* RTypeDeclContainerComponent::GetTypeMember(InRef<RName> name)
{
    auto i = typeDict.find(*name);
    if (i == typeDict.end()) return nullptr;

    return i->second;
}

} // namespace Citron