#pragma once


#include "RDecl.h"

namespace Citron {

class MEnumElemVarDecl;

class RType;
class RFactory;

class REnumElemVarDecl
    : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments& typeArgs, RFactory& factory) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class RMEnumElemVarDecl : public REnumElemVarDecl
{
    MEnumElemVarDecl* decl;
};


} // namespace Citron
