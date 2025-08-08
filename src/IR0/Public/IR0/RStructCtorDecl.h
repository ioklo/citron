#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MStructCtorDecl;

class RStructCtorDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RStructDecl* GetStructDecl() = 0;
    virtual RFuncParameter& GetUnboundFuncParam(size_t index) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMStructCtorDecl : public RStructCtorDecl
{
    MStructCtorDecl* decl;
};

} // namespace Citron
