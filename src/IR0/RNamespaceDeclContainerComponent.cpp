#include "RNamespaceDeclContainerComponent.h"
#include "RNamespaceDecl.h"

using namespace std;

namespace Citron {

void RNamespaceDeclContainerComponent::AddNamespace(shared_ptr<RNamespaceDecl> _namespace)
{
    namespaceDecls.push_back(_namespace);
    namespaceDict.insert_or_assign(_namespace->GetName(), std::move(_namespace));
}

shared_ptr<RNamespaceDecl> RNamespaceDeclContainerComponent::GetNamespace(const std::string& name)
{
    auto i = namespaceDict.find(name);
    if (i == namespaceDict.end()) return nullptr;

    return i->second;
}

}