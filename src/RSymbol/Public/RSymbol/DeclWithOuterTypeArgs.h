#pragma once

namespace Citron {

class RTypeArguments;

template<typename TDecl>
struct TDeclWithOuterTypeArgs
{
    TDecl* decl;
    RTypeArguments* outerTypeArgs;
};

} // namespace Citron