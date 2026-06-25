#pragma once
#include "RDecl.h"

namespace Citron {

// 일반적인 RFuncDeclBase
class RFuncDeclBase : public RDecl
{
public:
    virtual RThisKind GetThisKind() = 0;
    virtual size_t GetParamCount() = 0;
    virtual RType* GetReturnType(RTypeArguments* typeArgs) = 0;
    virtual RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) = 0;
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual std::span<RFuncParameter> GetUnboundFuncParams() = 0;
};

} // namespace Citron