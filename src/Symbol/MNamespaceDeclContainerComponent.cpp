module;
#include <memory>
#include <string>

module Citron.MDecls;

import :MDecl;
import :MNamespaceDecl;

using namespace std;

namespace Citron {

void MNamespaceDeclContainerComponent::AddNamespace(shared_ptr<MNamespaceDecl> _namespace)
{
    namespaceDecls.push_back(_namespace);
    namespaceDict.insert_or_assign(_namespace->GetName(), move(_namespace));
}

shared_ptr<MNamespaceDecl> MNamespaceDeclContainerComponent::GetNamespace(const std::string& name)
{
    auto i = namespaceDict.find(name);
    if (i == namespaceDict.end()) return nullptr;

    return i->second;
}

}