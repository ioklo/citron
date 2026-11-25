#pragma once

#include <vector>
#include <tuple>
#include <functional>
#include <expected>
#include "Infra/RefPtr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncReturn.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct RFuncParameter;
class RType;
class RDecl;
using RFactoryPtr = std::shared_ptr<RFactory>;
class SmPhase7Context;
class SmTypeResolveScope;

class SmPhase4Context
{
    RFactoryPtr rFactory;

public:
    SmPhase4Context(InRefPtr<RFactory> rFactory);

    template<typename TRDecl, typename... TArgs> requires std::derived_from<TRDecl, RDecl>
    TRDecl* MakeRDecl(TArgs&&... args)
    {
        return rFactory->MakeDecl<TRDecl>(std::forward<TArgs>(args)...);
    }
    
    std::expected<RFuncReturn, DiagPtr> MakeFuncReturn(SFuncReturn& funcRet, RDecl* funcDecl, std::span<RTypeParam*> typeParams, SmTypeResolveScope scope);
    std::expected<std::tuple<std::vector<RFuncParameter>, bool>, DiagPtr> MakeFuncParameters(std::span<SFuncParam> sParams, SmTypeResolveScope scope);
};

} // namespace Citron