#include "MNamespaceDeclContainerComponent.h"

#include <string>
#include "MDecl.h"
#include "MNamespaceDecl.h"

using namespace std;

namespace Citron {

void MNamespaceDeclContainerComponent::AddNamespace(MNamespaceDecl* _namespace)
{
    namespaceDecls.push_back(_namespace);
    namespaceDict.insert_or_assign(_namespace->GetName(), move(_namespace));
}

MNamespaceDecl* MNamespaceDeclContainerComponent::GetNamespace(const std::string& name)
{
    auto i = namespaceDict.find(name);
    if (i == namespaceDict.end()) return nullptr;

    return i->second;
}

}