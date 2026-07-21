#pragma once

namespace Citron {

class RTypeArguments;

// fully applied typeParam
template<typename TDecl>
struct RAppliedDecl
{
    TDecl* decl;
    RTypeArguments* typeArgs;
};

} // namespace Citron
