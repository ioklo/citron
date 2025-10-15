#pragma once

namespace Citron {

class RTypeArguments;

template<typename TDecl>
struct DeclWithOuterTypeArgs
{
    TDecl* decl;
    RTypeArguments* outerTypeArgs;
};

} // namespace Citron