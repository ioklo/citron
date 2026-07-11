#pragma once
#include <memory>
#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"

namespace Citron {

class NDecl;

class SynthesizeImplicitSymbolContext
{
    RFactoryPtr rFactory;

public:
    SynthesizeImplicitSymbolContext(TakeRef<RFactoryPtr> rFactory);

    template<typename TRDecl, typename... TArgs>
    TRDecl* MakeRDecl(TArgs&&... args)
    {
        return rFactory->MakeDecl<TRDecl>(std::forward<TArgs>(args)...);
    }
};

} // namespace Citron