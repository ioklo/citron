#pragma once

#include <memory>

#include "Symbol/MClassVarDecl.h"
#include "RDecl.h"

namespace Citron {

class MClassVarDecl;

class RType;
using RTypePtr = std::shared_ptr<RType>;

class RTypeFactory;

class RClassVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual bool IsStatic() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMClassVarDecl : public RClassVarDecl
{
    std::shared_ptr<MClassVarDecl> decl;
};


} // namespace Citron
