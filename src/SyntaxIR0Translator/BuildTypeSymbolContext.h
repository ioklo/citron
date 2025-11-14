#pragma once

#include <functional>
#include <memory>
#include <string>

#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"

namespace Citron {

class NNamespaceDecl;
using RFactoryPtr = std::shared_ptr<RFactory>;
using NFactoryPtr = std::shared_ptr<NFactory>;

namespace SyntaxIR0Translator {

class BuildTypeSymbolContext
{   
    NFactoryPtr nFactory;

public:
    BuildTypeSymbolContext(const NFactoryPtr& nFactory);

    template<typename TNDecl, typename... TArgs> requires std::derived_from<TNDecl, NDecl>
    TNDecl* MakeNDecl(TArgs&&... args)
    {
        return nFactory->MakeNDecl<TNDecl>(std::forward<TArgs>(args)...);
    }
};

} // SyntaxIR0Translator
} // Citron

