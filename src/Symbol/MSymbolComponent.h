#pragma once

#include "MType.h"

namespace Citron
{

// TOuter는 shared, weak or variant
template<typename TOuter, typename TDecl>
class MSymbolComponent
{
    std::weak_ptr<TOuter> outer;
    std::shared_ptr<TDecl> decl;
    std::vector<MTypePtr> typeArgs;
};

}