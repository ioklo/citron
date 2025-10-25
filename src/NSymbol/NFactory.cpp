#include "NFactory.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RNamespaceDeclGroup.h"
#include "NModule.h"
#include "NNamespaceDecl.h"

#include <cassert>

using namespace std;

namespace Citron {

NModule* NFactory::MakeNModule(string&& name)
{
    auto nModule = make_unique<NModule>(move(name));
    auto* pNModule = nModule.get();
    nModules.push_back(move(nModule));
    return pNModule;
}

NNamespaceDecl* NFactory::MakeRootNamespaceDecl()
{
    // root namespace면 
    auto* group = rFactory->GetNamespaceDeclGroup({});
    unique_ptr<NNamespaceDecl> newDecl{new NNamespaceDecl{nullptr, "", group}};
    auto pNewDecl = newDecl.get();
    namespaceDecls.push_back(move(newDecl));

    group->Add(pNewDecl);
    return pNewDecl;
}

NNamespaceDecl* NFactory::MakeChildNamespaceDecl(NNamespaceDecl* outer, const string& name)
{
    assert(outer && !name.empty());

    // root namespace면 
    vector<string> id;

    id.push_back(name);
    auto curNS = outer;

    while (curNS)
    {
        auto curOuter = curNS->outer;

        if (!curOuter)
        {
            // root 라면 그만둔다
            assert(curNS->name.empty());
            break;
        }

        id.push_back(curNS->name);
        curNS = curOuter;
    }

    reverse(id.begin(), id.end());

    auto group = rFactory->GetNamespaceDeclGroup(id);
    unique_ptr<NNamespaceDecl> newDecl{new NNamespaceDecl{outer, name, group}};
    auto pNewDecl = newDecl.get();
    namespaceDecls.push_back(move(newDecl));

    group->Add(pNewDecl);
    return pNewDecl;
}

} // namespace Citron