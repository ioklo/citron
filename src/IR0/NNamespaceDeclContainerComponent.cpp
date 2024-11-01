#include "NNamespaceDeclContainerComponent.h"
#include "NNamespaceDecl.h"

using namespace std;

namespace Citron {

NNamespaceDeclContainerComponent::NNamespaceDeclContainerComponent() = default;

void NNamespaceDeclContainerComponent::AddNamespace(shared_ptr<NNamespaceDecl> _namespace)
{
    namespaceDecls.push_back(_namespace);
    namespaceDict.insert_or_assign(_namespace->GetName(), std::move(_namespace));
}

shared_ptr<NNamespaceDecl> NNamespaceDeclContainerComponent::GetNamespace(const std::string& name)
{
    auto i = namespaceDict.find(name);
    if (i == namespaceDict.end()) return nullptr;

    return i->second;
}

optional<RMember> NNamespaceDeclContainerComponent::GetMemberNamespace(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto* normalName = get_if<RName_Normal>(&name);

    if (!normalName) return nullopt;
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    auto i = namespaceDict.find(normalName->text);
    if (i == namespaceDict.end()) return nullopt;

    return RMember_Namespace(i->second);
}

}