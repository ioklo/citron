#pragma once
#include "RSymbolConfig.h"

#include "Infra/AnyPtrSizedRange.h"
#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class REnumDecl : public RDecl, public RTypeDecl
{
public:
    virtual size_t GetTypeParamCount() = 0;
    virtual AnyPtrSizedRange<RTypeParamDecl*> GetTypeParams() = 0;

public: // from RTypeDecl
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API RDecl* GetDecl() override;
    RSYMBOL_API RType* GetOpenType() override;
    RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
};

} // namespace Citron
