#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <memory>
#include <string>

namespace Citron {

class RFactory;
class NModule;
class NNamespaceDecl;
class NDecl;

class NFactory
{
    RFactory* rFactory;

    std::vector<std::unique_ptr<NModule>> nModules;
    std::vector<std::unique_ptr<NNamespaceDecl>> namespaceDecls;
    std::vector<std::unique_ptr<NDecl>> nDecls;

public:
    NSYMBOL_API NFactory(RFactory* rFactory);
    NSYMBOL_API ~NFactory();
    NSYMBOL_API NModule* MakeNModule(std::string&& name);

    NSYMBOL_API NNamespaceDecl* MakeRootNamespaceDecl(); // TU당 하나씩 만들어지는 namespace
    NNamespaceDecl* MakeChildNamespaceDecl(NNamespaceDecl* outer, const std::string& name);

    template<typename TNDecl, typename... TArgs> requires std::derived_from<TNDecl, NDecl> && (!std::same_as<TNDecl, NNamespaceDecl>)
        TNDecl* MakeNDecl(TArgs&&... args)
    {
        auto decl = std::make_unique<TNDecl>(std::forward<TArgs>(args)...);
        auto* pDecl = decl.get();
        nDecls.push_back(std::move(decl));
        return pDecl;
    }
};

using NFactoryPtr = std::shared_ptr<NFactory>;

} // namespace Citron