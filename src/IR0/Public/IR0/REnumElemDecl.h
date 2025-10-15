#pragma once

#include <optional>

#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class MEnumElemDecl;

struct RFuncParameter;

class RTypeArguments;

class REnumElemDecl
    : public RDecl
    , public RTypeDecl
{
public:
    virtual REnumDecl* GetBaseEnumDecl() = 0;
    virtual std::optional<RMember_EnumElemVar> GetVar(RTypeArguments* typeArgs, const RName& name) = 0;
    virtual size_t GetVarCount() = 0;
    virtual bool IsStandalone() = 0;
    virtual std::vector<RFuncParameter> GetUnboundCtorParams() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(this); }
};

class RMEnumElemDecl : public REnumElemDecl
{
    MEnumElemDecl* decl;
};


} // namespace Citron
