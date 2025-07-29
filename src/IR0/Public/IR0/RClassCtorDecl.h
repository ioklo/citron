#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MClassCtorDecl;

class RClassCtorDecl
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

class RMClassCtorDecl : public RClassCtorDecl
{
    std::shared_ptr<MClassCtorDecl> decl;
};


} // namespace Citron
