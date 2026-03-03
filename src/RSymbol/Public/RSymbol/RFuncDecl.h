#pragma once

#include "RSymbolConfig.h"

#include <optional>
#include <span>

#include "RDecl.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"

namespace Citron {

class RType;
class RFactory;

struct RFuncDeclVisitor;

class EFuncDecl;

enum class RThisKind
{
    None,   // static
    Ptr,    // ref
    Handle, // class  
};

class RFuncDecl
{
public:
    virtual ~RFuncDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual RThisKind GetThisKind() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual RTypeParamDecl* GetTypeParam(size_t index) = 0;
    virtual size_t GetParamCount() = 0;
    virtual RType* GetReturnType(RTypeArguments& typeArgs) = 0;
    virtual RFuncReturn GetFuncReturn(RTypeArguments& typeArgs) = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index) = 0;
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual std::span<RFuncParameter> GetUnboundFuncParams() = 0;
    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

class REFuncDecl : public RFuncDecl
{
    EFuncDecl* funcDecl;
};

} // namespace Citron

#include "RFuncDeclVisitor.g.h"
