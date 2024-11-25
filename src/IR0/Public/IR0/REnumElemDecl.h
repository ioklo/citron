#pragma once

#include <memory>
#include <optional>

#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class MEnumElemDecl;

class REnumElemDecl
    : public RDecl
    , public RTypeDecl
{
public:    
    virtual std::optional<RMember_EnumElemMemberVar> GetMemberVar(const RTypeArgumentsPtr& typeArgs, const RName& name) = 0;
    virtual size_t GetMemberVarCount() = 0;
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
