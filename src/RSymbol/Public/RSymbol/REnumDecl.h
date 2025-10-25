#pragma once


#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class EEnumDecl;

class REnumDecl
    : public RDecl
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REEnumDecl : public REnumDecl
{
    EEnumDecl* decl;
};


} // namespace Citron
