#pragma once

#include <memory>
#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MClassMemberFuncDecl;
using RTypePtr = std::shared_ptr<class RType>;
class RTypeArguments;
class RTypeFactory;

class RClassMemberFuncDecl
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

class RMClassMemberFuncDecl : public RClassMemberFuncDecl
{
    std::shared_ptr<MClassMemberFuncDecl> decl;
};


} // namespace Citron
