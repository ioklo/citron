#pragma once
#include <memory>
#include "NSymbol/NFactory.h"

namespace Citron {

class NDecl;
using NFactoryPtr = std::shared_ptr<class NFactory>;

class SynthesizeImplicitSymbolContext
{
    NFactoryPtr nFactory;

public:
    SynthesizeImplicitSymbolContext(const NFactoryPtr& nFactory);

    template<typename TNDecl, typename... TArgs> requires std::derived_from<TNDecl, NDecl>
    TNDecl* MakeNDecl(TArgs&&... args)
    {
        return nFactory->MakeNDecl<TNDecl>(std::forward<TArgs>(args)...);
    }

};

} // namespace Citron