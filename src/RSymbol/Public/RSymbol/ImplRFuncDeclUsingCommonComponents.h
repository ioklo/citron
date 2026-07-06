#pragma once
#include <span>
#include "RFuncDecl.h"
#include "RThisKind.h"
#include "RFuncReturn.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron {

class RDecl;
class RTypeParamDecl;
class RTypeArguments;
struct RFuncParameter;
class RCommonFuncDeclComponent;
class RGenericsComponent;

class ImplRFuncDeclUsingCommonComponents : public RFuncDecl
{
    RDecl* decl; 
    RCommonFuncDeclComponent& commonFuncDeclComp;

public:
    ImplRFuncDeclUsingCommonComponents(RDecl* decl, RCommonFuncDeclComponent& commonFuncDeclComp)
        : decl{decl}, commonFuncDeclComp{commonFuncDeclComp}
    { }
    RDecl* RFuncDecl_GetDecl() final { return decl; }
    RThisKind GetThisKind() final { return commonFuncDeclComp.GetThisKind(); }
    size_t GetParamCount() final { return commonFuncDeclComp.GetParamCount(); }
    RType* GetReturnType(RTypeArguments* typeArgs) final { return commonFuncDeclComp.GetReturnType(typeArgs); }
    RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) final { return commonFuncDeclComp.GetFuncReturn(typeArgs); }
    RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) final { return commonFuncDeclComp.GetFuncParam(typeArgs, index); }
    RFuncReturn GetUnboundFuncReturn() final { return commonFuncDeclComp.GetUnboundFuncReturn(); }
    std::span<RFuncParameter> GetUnboundFuncParams() final { return commonFuncDeclComp.GetUnboundFuncParams(); }
};

} // namespace Citron
