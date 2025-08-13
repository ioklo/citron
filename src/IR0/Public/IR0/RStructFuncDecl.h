#pragma once


#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MStructFuncDecl;

class RType;
class IR0Factory;

class RStructFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RType* GetReturnType(RTypeArguments& typeArgs, IR0Factory& factory) = 0;
    virtual bool IsStatic() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class RMStructFuncDecl : public RStructFuncDecl
{
    MStructFuncDecl* decl;
};


} // namespace Citron
