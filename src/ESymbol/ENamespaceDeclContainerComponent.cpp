#include "ENamespaceDeclContainerComponent.h"

#include <string>
#include "EDecl.h"
#include "ENamespaceDecl.h"

using namespace std;

namespace Citron {

void ENamespaceDeclContainerComponent::AddNamespace(ENamespaceDecl* _namespace)
{
    namespaceDecls.push_back(_namespace);
    namespaceDict.insert_or_assign(_namespace->GetName(), move(_namespace));
}

ENamespaceDecl* ENamespaceDeclContainerComponent::GetNamespace(const std::string& name)
{
    auto i = namespaceDict.find(name);
    if (i == namespaceDict.end()) return nullptr;

    return i->second;
}

}