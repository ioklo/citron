#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RFuncDecl.h"

namespace Citron {

class EClassCtorDecl;

class RClassCtorDecl
    : public RDecl
    , public RFuncDecl
{
public:
    virtual RClassDecl* GetClassDecl() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RFuncDeclVisitor& visitor) final;
};

class REClassCtorDecl : public RClassCtorDecl
{
    EClassCtorDecl* decl;
};


} // namespace Citron
