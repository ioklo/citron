#pragma once
#include "RSymbolConfig.h"
#include <optional>
#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

struct RFuncParameter;

class RTypeArguments;

class REnumElemDecl : public RDecl, public RTypeDecl
{
public:
    virtual REnumDecl* GetBaseEnumDecl() = 0;
    virtual std::optional<RDeclRes_EnumElemVar> GetVar(RTypeArguments* typeArgs, const RName& name) = 0;
    virtual REnumElemVarDecl* GetVarDecl(size_t index) = 0;
    virtual size_t GetVarCount() = 0;
    virtual bool IsStandalone() = 0;

public: // from RTypeDecl
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API RDecl* GetDecl() override;
    RSYMBOL_API RType* GetOpenType() override;
    RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
};

} // namespace Citron
