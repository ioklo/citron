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

}