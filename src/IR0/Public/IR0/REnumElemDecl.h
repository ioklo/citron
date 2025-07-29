#pragma once

#include <memory>
#include <optional>

#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class MEnumElemDecl;

struct RFuncParameter;

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

class REnumElemDecl
    : public RDecl
    , public RTypeDecl
{
public:
    virtual std::shared_ptr<REnumDecl> GetBaseEnumDecl() = 0;
    virtual std::optional<RMember_EnumElemVar> GetVar(const RTypeArgumentsPtr& typeArgs, const RName& name) = 0;
    virtual size_t GetVarCount() = 0;
    virtual bool IsStandalone() = 0;
    virtual std::vector<RFuncParameter> GetUnboundCtorParams() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMEnumElemDecl : public REnumElemDecl
{
    std::shared_ptr<MEnumElemDecl> decl;
};


} // namespace Citron
