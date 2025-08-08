#pragma once

#include <memory>

#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MGlobalFuncDecl;

class RType;
class RTypeFactory;

// abstract
class RGlobalFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RType* GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

class RMGlobalFuncDecl : public RGlobalFuncDecl
{
    MGlobalFuncDecl* externalFuncDecl;
};

}