#pragma once

#include "RDecl.h"

namespace Citron {

class EClassVarDecl;

class RType;
class RFactory;

class RClassVarDecl
    : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments& typeArgs) = 0;
    virtual bool IsStatic() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REClassVarDecl : public RClassVarDecl
{
    EClassVarDecl* decl;
};


} // namespace Citron
