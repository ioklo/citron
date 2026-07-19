#pragma once
#include <vector>

namespace Citron {

class RTypeArguments;

template<typename TDecl>
struct ROuterAppliedDecl
{
    RTypeArguments* outerTypeArgs;
    TDecl* decl;
};

template<typename TFuncDecl>
struct ROuterAppliedFuncDeclGroup
{
    RTypeArguments* outerTypeArgs;
    std::vector<TFuncDecl*> decls;
};

} // namespace Citron
