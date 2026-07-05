#pragma once
#include <memory>
#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"

namespace Citron {

class NDecl;
using NFactoryPtr = std::shared_ptr<class NFactory>;

class SynthesizeImplicitSymbolContext
{
    NFactoryPtr nFactory;
    RFactoryPtr rFactory;

public:
    SynthesizeImplicitSymbolContext(const NFactoryPtr& nFactory);

    template<typename TRDecl, typename... TArgs>
    TRDecl* MakeRDecl(TArgs&&... args)
    {
        return rFactory->MakeRDecl<TRDecl>(std::forward<TArgs>(args)...);
    }

    template<typename TNDecl, typename... TArgs> requires std::derived_from<TNDecl, NDecl>
    TNDecl* MakeNDecl(TArgs&&... args)
    {
        return nFactory->MakeNDecl<TNDecl>(std::forward<TArgs>(args)...);
    }

};

} // namespace Citron