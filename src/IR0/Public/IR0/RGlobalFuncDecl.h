#pragma once


#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MGlobalFuncDecl;

class RType;
class IR0Factory;

// abstract
class RGlobalFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RType* GetReturnType(RTypeArguments& typeArgs, IR0Factory& factory) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class RMGlobalFuncDecl : public RGlobalFuncDecl
{
    MGlobalFuncDecl* externalFuncDecl;
};

}