#pragma once

#include <vector>
#include <tuple>
#include <functional>
#include <expected>

#include "Syntax/Syntax.h"
#include "NSymbol/NFactory.h"

namespace Citron {

struct RFuncParameter;
class RType;
class NDecl;
using RFactoryPtr = std::shared_ptr<RFactory>;
using NFactoryPtr = std::shared_ptr<NFactory>;
using DiagPtr = std::shared_ptr<struct Diag>;

class TranslateBodyContext;

class BuildTypeDependentSymbolContext
{
    RFactoryPtr rFactory;
    NFactoryPtr nFactory;

public:
    BuildTypeDependentSymbolContext(const RFactoryPtr& rFactory, const NFactoryPtr& nFactory);

    template<typename TNDecl, typename... TArgs> requires std::derived_from<TNDecl, NDecl>
    TNDecl* MakeNDecl(TArgs&&... args)
    {
        return nFactory->MakeNDecl<TNDecl>(std::forward<TArgs>(args)...);
    }

    RType* MakeType(STypeExp* sTypeExp, NDecl* decl);
    std::expected<std::tuple<std::vector<RFuncParameter>, bool>, DiagPtr> MakeParameters(NDecl* decl, std::vector<SFuncParam>& sParams);
};

} // namespace Citron