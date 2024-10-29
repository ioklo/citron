#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MStructMemberFuncDecl;

class RStructMemberFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMStructMemberFuncDecl : public RStructMemberFuncDecl
{
    std::shared_ptr<MStructMemberFuncDecl> decl;
};


} // namespace Citron
