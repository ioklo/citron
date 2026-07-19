#pragma once

#include <memory>
#include <vector>
#include "RSymbol/ROuterAppliedDecl.h"
#include "RSymbol/RFuncDecl.h"

namespace Citron {

class RTypeArguments;

template<typename TFuncDecl> requires std::derived_from<TFuncDecl, RFuncDecl>
struct SmPartiallyAppliedFuncDeclGroup : ROuterAppliedFuncDeclGroup<TFuncDecl>
{
    RTypeArguments* memberTypeArgs; // outer부분을 제외한 typeArgs면서 완전하지 않을수도 있는 typeArgs
};

} // namespace Citron