#pragma once

#include <vector>
#include <tuple>
#include <functional>
#include <expected>
#include "Infra/Ref.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncReturn.h"

namespace Citron {

struct RFuncParameter;
class RType;
class RDecl;
using RFactoryPtr = std::shared_ptr<RFactory>;
using DiagPtr = std::shared_ptr<struct Diag>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class TranslateBodyContext;
class SmTypeResolveScope;

class BuildNonTypeSymbolContext
{
    RFactoryPtr rFactory;

public:
    BuildNonTypeSymbolContext(TakeRef<RFactoryPtr> rFactory);

    template<typename TRDecl, typename... TArgs> requires std::derived_from<TRDecl, RDecl>
    TRDecl* MakeRDecl(TArgs&&... args)
    {
        return rFactory->MakeDecl<TRDecl>(std::forward<TArgs>(args)...);
    }
    
    std::expected<RFuncReturn, DiagPtr> MakeFuncReturn(SFuncReturn& funcRet, RDecl* funcDecl, std::span<RTypeParam*> typeParams, SmTypeResolveScope scope);
    std::expected<std::tuple<std::vector<RFuncParameter>, bool>, DiagPtr> MakeParameters(std::vector<SFuncParam>& sParams, SmTypeResolveScope scope);
};

} // namespace Citron