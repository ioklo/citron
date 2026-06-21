#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RFuncDecl.h"

namespace Citron {

class EStructDtorDecl;

class RStructDtorDecl
    : public RDecl
    , public RFuncDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RFuncDeclVisitor& visitor) final;
};

class REStructDtorDecl : public RStructDtorDecl
{
    EStructDtorDecl* decl;
};


} // namespace Citron