#include "RNamespaceDeclContainerComponent.h"

#include "RNamespaceDecl.h"

using namespace std;

namespace Citron {

RNamespaceDeclContainerComponent::RNamespaceDeclContainerComponent() = default;

void RNamespaceDeclContainerComponent::AddNamespace(RNamespaceDecl* _namespace)
{
    namespaceDecls.push_back(_namespace);
    namespaceDict.insert_or_assign(_namespace->GetName(), _namespace);
}

RNamespaceDecl* RNamespaceDeclContainerComponent::GetNamespace(InRef<RName> name)
{
    auto i = namespaceDict.find(*name);
    if (i == namespaceDict.end()) return nullptr;

    return i->second;
}

optional<RDeclRes> RNamespaceDeclContainerComponent::GetMemberNamespace(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{   
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto i = namespaceDict.find(*name);
    if (i == namespaceDict.end()) return nullopt;

    return RDeclRes_Namespace(i->second);
}

}