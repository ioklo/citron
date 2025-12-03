#pragma once


#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class EGlobalFuncDecl;

class RType;
class RFactory;

// abstract
class RGlobalFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RType* GetReturnType(RTypeArguments& typeArgs, RFactory& factory) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class REGlobalFuncDecl : public RGlobalFuncDecl
{
    EGlobalFuncDecl* externalFuncDecl;
};

}