#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class EStructDtorDecl;

class RStructDtorDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class REStructDtorDecl : public RStructDtorDecl
{
    EStructDtorDecl* decl;
};


} // namespace Citron