#pragma once

#include <memory>

#include "RDecl.h"

namespace Citron {

class MEnumElemVarDecl;

class RType;
class RTypeFactory;

class REnumElemVarDecl
    : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMEnumElemVarDecl : public REnumElemVarDecl
{
    MEnumElemVarDecl* decl;
};


} // namespace Citron
