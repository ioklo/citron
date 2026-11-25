#pragma once
#include <memory>
#include "Infra/RefPtr.h"
#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"

namespace Citron {

class NDecl;

class SmPhase6Context
{
    RFactoryPtr rFactory;

public:
    SmPhase6Context(InRefPtr<RFactory> rFactory);

    template<typename TRDecl, typename... TArgs>
    TRDecl* MakeRDecl(TArgs&&... args)
    {
        return rFactory->MakeDecl<TRDecl>(std::forward<TArgs>(args)...);
    }
};

} // namespace Citron