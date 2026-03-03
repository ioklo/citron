#pragma once
#include "RSymbolConfig.h"
#include <optional>

#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class EEnumElemDecl;

struct RFuncParameter;

class RTypeArguments;

class REnumElemDecl
    : public RDecl
    , public RTypeDecl
{
public:
    virtual REnumDecl* GetBaseEnumDecl() = 0;
    virtual std::optional<RDeclRes_EnumElemVar> GetVar(RTypeArguments* typeArgs, const RName& name) = 0;
    virtual REnumElemVarDecl* GetVarDecl(size_t index) = 0;
    virtual size_t GetVarCount() = 0;
    virtual bool IsStandalone() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

class REEnumElemDecl : public REnumElemDecl
{
    EEnumElemDecl* decl;
};


} // namespace Citron
