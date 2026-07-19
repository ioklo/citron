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
    typeDict.insert_or_assign(typeDecl->RTypeDecl_GetDecl()->GetIdentifier().name, typeDecl);
}

RTypeDecl* RTypeDeclContainerComponent::GetTypeMember(InRef<RName> name)
{
    auto i = typeDict.find(*name);
    if (i == typeDict.end()) return nullptr;

    return i->second;
}

} // namespace Citron