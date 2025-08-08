#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class MStructDecl;

class RTypeArguments;

class RStructDecl
    : public RDecl
    , public RFuncDeclOuter
    , public RTypeDecl
    , public RTypeDeclOuter
{
public:
    virtual std::optional<RMember_StructVar> GetVar(RTypeArguments* typeArgs, const RName& name) = 0;
    virtual std::vector<RStructCtorDecl*> GetUnboundCtors() = 0;
    virtual RStructCtorDecl* GetUnboundTrivialCtor_RStructCtorDecl() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMStructDecl : public RStructDecl
{
    MStructDecl* decl;
};


} // namespace Citron

