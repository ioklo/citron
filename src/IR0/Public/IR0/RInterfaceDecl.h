#pragma once


#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class MInterfaceDecl;

class RInterfaceDecl
    : public RDecl
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(this); }
};

class RMInterfaceDecl : public RInterfaceDecl
{
    MInterfaceDecl* decl;
};


} // namespace Citron

