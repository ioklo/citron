#pragma once

namespace Citron {

class RTypeArguments;

// fully applied decl
template<typename TDecl>
struct RAppliedDecl
{
    TDecl* decl;
    RTypeArguments* typeArgs;
};

} // namespace Citron
