#pragma once
#include "RSymbolConfig.h"

#include <span>

#include "RFuncParameter.h"

namespace Citron {

class RDecl;
class RThisKind;
class RType;
class RTypeArguments;
class RTypeParamDecl;
class RFuncReturn;

class RFuncDecl
{
public:
    virtual ~RFuncDecl() = default;
    
    virtual RDecl* GetDecl() = 0;
    virtual RThisKind GetThisKind() = 0;
    // virtual size_t GetTypeParamCount() = 0;
    // virtual RTypeParamDecl* GetTypeParam(size_t index) = 0;
    virtual size_t GetParamCount() = 0;
    virtual RType* GetReturnType(RTypeArguments* typeArgs) = 0;
    virtual RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) = 0;
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual std::span<RFuncParameter> GetUnboundFuncParams() = 0;
};

} // namespace Citron

