#pragma once

namespace Citron {

class RFuncDeclBase;
class RTypeArguments;

template<typename TDecl>
struct TDeclWithOuterTypeArgs
{
    TDecl* decl;
    RTypeArguments* outerTypeArgs;
};

} // namespace Citron