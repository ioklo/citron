#pragma once

namespace Citron {

class RFuncDeclBase;
class RTypeArguments;

struct DeclWithOuterTypeArgs
{
    RFuncDeclBase* funcDecl;
    RTypeArguments* outerTypeArgs;
};

template<typename TDecl>
struct TDeclWithOuterTypeArgs
{
    TDecl* decl;
    RTypeArguments* outerTypeArgs;
};

} // namespace Citron