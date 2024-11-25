#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MStructCtorDecl;
class RStructDecl;

class RStructCtorDecl 
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual std::shared_ptr<RStructDecl> GetStructDecl() = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMStructCtorDecl : public RStructCtorDecl
{
    std::shared_ptr<MStructCtorDecl> decl;
};


} // namespace Citron
