#pragma once
#include "RSymbolConfig.h"

#include "Infra/AnyPtrSizedRange.h"
#include "RDecl.h"
#include "RFuncDecl.h"

namespace Citron {

class RClassDecl;
class EClassCtorDecl;

class RClassCtorDecl : public RDecl, public RFuncDecl
{
public:
    virtual RClassDecl* GetClassDecl() = 0;

    // for RHasTypeParams
    virtual size_t GetTypeParamCount() = 0;
    virtual AnyPtrSizedRange<RTypeParamDecl*> GetTypeParams() = 0;

public: // from RFuncDecl
    RSYMBOL_API RDecl* GetDecl() override;
    RSYMBOL_API RThisKind GetThisKind() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API size_t GetParamCount() override;
    RSYMBOL_API RType* GetReturnType(RTypeArguments* typeArgs) override;
    RSYMBOL_API RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) override;
    RSYMBOL_API RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) override;
    RSYMBOL_API RFuncReturn GetUnboundFuncReturn() override;
    RSYMBOL_API std::span<RFuncParameter> GetUnboundFuncParams() override;
};

} // namespace Citron
