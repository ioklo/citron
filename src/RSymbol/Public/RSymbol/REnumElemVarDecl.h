#pragma once


#include "RDecl.h"

namespace Citron {

class EEnumElemVarDecl;

class RType;
class RFactory;

class REnumElemVarDecl
    : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments* typeArgs) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REEnumElemVarDecl : public REnumElemVarDecl
{
    EEnumElemVarDecl* decl;
};


} // namespace Citron
