#pragma once

#include <memory>

namespace Citron {

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

template<typename TDecl>
struct RDeclWithOuterTypeArgs
{
    std::shared_ptr<TDecl> decl;
    RTypeArgumentsPtr outerTypeArgs;
};

} // namespace Citron