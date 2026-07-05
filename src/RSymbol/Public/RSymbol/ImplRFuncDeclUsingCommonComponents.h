#pragma once
#include <span>
#include "RFuncDecl.h"
#include "RThisKind.h"
#include "RFuncReturn.h"

namespace Citron {

class RDecl;
class RTypeParamDecl;
class RTypeArguments;
struct RFuncParameter;
class RCommonFuncDeclComponent;
class RGenericsComponent;

template<typename TRFuncDecl>
class ImplRFuncDeclUsingCommonComponents : public RFuncDecl
{
public:
    RDecl* GetDecl() final { return (TRFuncDecl*)this; }
    RThisKind GetThisKind() final { return ((TRFuncDecl*)this)->commonFuncDeclComp.GetThisKind(); }
    size_t GetParamCount() final { return ((TRFuncDecl*)this)->commonFuncDeclComp.GetParamCount(); }
    RType* GetReturnType(RTypeArguments* typeArgs) final { return ((TRFuncDecl*)this)->commonFuncDeclComp.GetReturnType(typeArgs); }
    RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) final { return ((TRFuncDecl*)this)->commonFuncDeclComp.GetFuncReturn(typeArgs); }
    RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) final { return ((TRFuncDecl*)this)->commonFuncDeclComp.GetFuncParam(typeArgs, index); }
    RFuncReturn GetUnboundFuncReturn() final { return ((TRFuncDecl*)this)->commonFuncDeclComp.GetUnboundFuncReturn(); }
    std::span<RFuncParameter> GetUnboundFuncParams() final { return ((TRFuncDecl*)this)->commonFuncDeclComp.GetUnboundFuncParams(); }
};

} // namespace Citron
