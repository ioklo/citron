#pragma once


#include "RDecl.h"

namespace Citron {

class MStructVarDecl;

class RType;
class IR0Factory;

class RStructVarDecl
    : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments& typeArgs, IR0Factory& factory) = 0;
    virtual bool IsStatic() = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class RMStructVarDecl : public RStructVarDecl
{
    MStructVarDecl* decl;
};


} // namespace Citron
