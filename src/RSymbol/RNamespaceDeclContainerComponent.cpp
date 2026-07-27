#include "RNamespaceDeclContainerComponent.h"

#include "RNamespace.h"

using namespace std;

namespace Citron {

RNamespaceDeclContainerComponent::RNamespaceDeclContainerComponent() = default;

void RNamespaceDeclContainerComponent::AddNamespace(RNamespace* _namespace)
{
    namespaces.push_back(_namespace);
    namespaceDict.insert_or_assign(_namespace->GetName(), _namespace);
}

RNamespace* RNamespaceDeclContainerComponent::GetNamespace(InRef<RName> name)
{
    auto i = namespaceDict.find(*name);
    if (i == namespaceDict.end()) return nullptr;

    return i->second;
}
}