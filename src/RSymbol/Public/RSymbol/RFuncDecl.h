#pragma once
#include "RSymbolConfig.h"

#include <span>

#include "RFuncParameter.h"
#include "RThisKind.h"

namespace Citron {

class RDecl;
class RThisKind;
class RType;
class RTypeArguments;
class RTypeParamDecl;
class RFuncReturn;

class RGlobalFuncDecl;
class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RLambdaDecl;
struct RFuncDeclVisitor;

class RFuncDecl
{
public:
    virtual ~RFuncDecl() = default;
    
    bool IsStatic() { return GetThisKind().IsStatic(); }
    
    virtual RDecl* RFuncDecl_GetDecl() = 0;
    virtual RThisKind GetThisKind() = 0;
    virtual size_t GetParamCount() = 0;
    virtual RType* GetReturnType(RTypeArguments* typeArgs) = 0;
    virtual RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) = 0;
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual std::span<RFuncParameter> GetUnboundFuncParams() = 0;
    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

} // namespace Citron

#include "RFuncDeclVisitor.g.h"