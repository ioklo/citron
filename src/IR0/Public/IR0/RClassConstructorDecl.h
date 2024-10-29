#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MClassConstructorDecl;

class RClassConstructorDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual std::shared_ptr<RClassDecl> GetClassDecl() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMClassConstructorDecl : public RClassConstructorDecl
{
    std::shared_ptr<MClassConstructorDecl> decl;
};


} // namespace Citron
