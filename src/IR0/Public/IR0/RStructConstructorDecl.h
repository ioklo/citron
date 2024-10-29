#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MStructConstructorDecl;
class RStructDecl;

class RStructConstructorDecl 
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

class RMStructConstructorDecl : public RStructConstructorDecl
{
    std::shared_ptr<MStructConstructorDecl> decl;
};


} // namespace Citron
