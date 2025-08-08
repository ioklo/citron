#pragma once

#include <memory>

#include "RDecl.h"

namespace Citron {

class MStructVarDecl;

class RType;
class RTypeFactory;

class RStructVarDecl
    : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual bool IsStatic() = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMStructVarDecl : public RStructVarDecl
{
    MStructVarDecl* decl;
};


} // namespace Citron
