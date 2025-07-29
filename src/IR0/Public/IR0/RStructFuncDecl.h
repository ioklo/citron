#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MStructFuncDecl;

class RType;
using RTypePtr = std::shared_ptr<RType>;

class RTypeFactory;

class RStructFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual bool IsStatic() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMStructFuncDecl : public RStructFuncDecl
{
    std::shared_ptr<MStructFuncDecl> decl;
};


} // namespace Citron
