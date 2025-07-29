#pragma once

#include <memory>

namespace Citron {

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

template<typename TDecl>
struct DeclWithOuterTypeArgs
{
    std::shared_ptr<TDecl> decl;
    RTypeArgumentsPtr outerTypeArgs;
};

} // namespace Citron