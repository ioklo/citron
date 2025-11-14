#pragma once

#include <vector>
#include <tuple>
#include <functional>

#include "Syntax/Syntax.h"
#include "NSymbol/NFactory.h"

namespace Citron {

struct RFuncParameter;
class RType;
class NDecl;
using NFactoryPtr = std::shared_ptr<NFactory>;

namespace SyntaxIR0Translator {

class TranslateBodyContext;

class BuildTypeDependentSymbolContext
{
    NFactoryPtr nFactory;

public:
    BuildTypeDependentSymbolContext(const NFactoryPtr& nFactory);

    template<typename TNDecl, typename... TArgs> requires std::derived_from<TNDecl, NDecl>
    TNDecl* MakeNDecl(TArgs&&... args)
    {
        return nFactory->MakeNDecl<TNDecl>(std::forward<TArgs>(args)...);
    }

    RType* MakeType(STypeExp* sTypeExp, NDecl* decl);
    std::tuple<std::vector<RFuncParameter>, bool> MakeParameters(NDecl* decl, std::vector<SFuncParam>& sParams);
};

} // namespace SyntaxIR0Translator
} // namespace Citron