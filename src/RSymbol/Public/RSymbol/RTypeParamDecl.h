#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class RTypeParamDecl : public RDecl, public RTypeDecl
{
    RName name;

public:
    RName& GetName() { return name; }

    virtual ~RTypeParamDecl() = default;
    virtual size_t GetGlobalIndex() = 0;

public: // from RTypeDecl
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API RDecl* GetDecl() override;
    RSYMBOL_API RType* GetOpenType() override;
    RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
};


} // namespace Citron
