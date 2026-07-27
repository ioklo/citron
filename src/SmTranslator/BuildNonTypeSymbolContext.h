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

class TranslateBodyContext;

struct SmFuncHeaderResolveScope
{
    RDecl* outer;
    std::span<RTypeParam*> typeParams;
};

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

    RType* MakeType(STypeExp* sTypeExp, RDecl* scope);
    RType* MakeType(STypeExp* sTypeExp, InRef<SmFuncHeaderResolveScope> scope);
    std::expected<RFuncReturn, DiagPtr> MakeFuncReturn(SFuncReturn& funcRet, InRef<SmFuncHeaderResolveScope> scope);
    std::expected<std::tuple<std::vector<RFuncParameter>, bool>, DiagPtr> MakeParameters(std::vector<SFuncParam>& sParams, InRef<SmFuncHeaderResolveScope> scope);
};

} // namespace Citron