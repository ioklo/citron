#pragma once

#include <memory>

namespace Citron {

using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

template<typename TDecl>
struct DeclWithOuterTypeArgs
{
    std::shared_ptr<TDecl> decl;
    RTypeArgumentsPtr outerTypeArgs;
};

} // namespace Citron