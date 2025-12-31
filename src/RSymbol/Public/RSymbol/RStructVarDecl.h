#pragma once


#include "RDecl.h"

namespace Citron {

class EStructVarDecl;

class RType;
class RFactory;

class RStructVarDecl
    : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments& typeArgs) = 0;
    virtual bool IsStatic() = 0;
    virtual size_t GetIndex() = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REStructVarDecl : public RStructVarDecl
{
    EStructVarDecl* decl;
};


} // namespace Citron
