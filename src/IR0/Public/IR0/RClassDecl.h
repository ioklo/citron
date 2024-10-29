#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class MClassDecl;

class RClassDecl
    : public RDecl
    , public RFuncDeclOuter
    , public RTypeDecl
    , public RTypeDeclOuter
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMClassDecl : public RClassDecl
{
    std::shared_ptr<MClassDecl> decl;
};


} // namespace Citron