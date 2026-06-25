#pragma once
#include "RSymbolConfig.h"

#include "RFuncDeclBase.h"

namespace Citron {

class EClassCtorDecl;

class RClassCtorDecl : public RFuncDeclBase
{
public:
    virtual RClassDecl* GetClassDecl() = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REClassCtorDecl : public RClassCtorDecl
{
    EClassCtorDecl* decl;
};


} // namespace Citron
