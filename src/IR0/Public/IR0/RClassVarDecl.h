#pragma once


#include "Symbol/MClassVarDecl.h"
#include "RDecl.h"

namespace Citron {

class MClassVarDecl;

class RType;
class RFactory;

class RClassVarDecl
    : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments& typeArgs, RFactory& factory) = 0;
    virtual bool IsStatic() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class RMClassVarDecl : public RClassVarDecl
{
    MClassVarDecl* decl;
};


} // namespace Citron
