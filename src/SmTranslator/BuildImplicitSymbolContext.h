#pragma once
#include <memory>
#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"

namespace Citron {

class NDecl;

class BuildImplicitSymbolContext
{
    RFactoryPtr rFactory;

public:
    BuildImplicitSymbolContext(TakeRef<RFactoryPtr> rFactory);

    template<typename TRDecl, typename... TArgs>
    TRDecl* MakeRDecl(TArgs&&... args)
    {
        return rFactory->MakeDecl<TRDecl>(std::forward<TArgs>(args)...);
    }
};

} // namespace Citron