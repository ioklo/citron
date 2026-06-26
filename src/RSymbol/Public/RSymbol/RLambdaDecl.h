#pragma once
#include "RSymbolConfig.h"

#include "RTypeDecl.h"
#include "RFuncDeclBase.h"

namespace Citron {

class RType;
class RFactory;

class RLambdaDecl
    : public RFuncDeclBase
    , public RTypeDecl
{
public:
    virtual RThisKind GetThisKind() = 0;
    virtual size_t GetParamCount() = 0;
    virtual RType* GetReturnType(RTypeArguments* typeArgs) = 0;
    virtual RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) = 0;
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual std::span<RFuncParameter> GetUnboundFuncParams() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

// M버전이 없다

} // namespace Citron
