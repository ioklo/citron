#pragma once

#include <memory>
#include <vector>

#include "RType.h"

namespace Citron
{

// TOuter는 shared, weak or variant
template<typename TOuter, typename TDecl>
class RSymbolComponent
{
    std::weak_ptr<TOuter> outer;
    std::shared_ptr<TDecl> decl;
    std::vector<RTypePtr> typeArgs;
};

}