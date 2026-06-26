#pragma once
#include "RSymbolConfig.h"

#include "RFuncDeclBase.h"

namespace Citron {

class EClassFuncDecl;

class RType;
class RFactory;

class RClassFuncDecl : public RFuncDeclBase
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REClassFuncDecl : public RClassFuncDecl
{
    EClassFuncDecl* decl;
};


} // namespace Citron
