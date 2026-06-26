#pragma once

#include "RFuncDeclBase.h"

namespace Citron {

class EStructDtorDecl;

class RStructDtorDecl : public RFuncDeclBase
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REStructDtorDecl : public RStructDtorDecl
{
    EStructDtorDecl* decl;
};


} // namespace Citron