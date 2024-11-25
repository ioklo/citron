#pragma once

#include <memory>

#include "RDecl.h"

namespace Citron {

class MStructVarDecl;

class RStructVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual bool IsStatic() = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMStructVarDecl : public RStructVarDecl
{
    std::shared_ptr<MStructVarDecl> decl;
};


} // namespace Citron
