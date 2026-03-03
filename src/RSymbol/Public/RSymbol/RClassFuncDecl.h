#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class EClassFuncDecl;

class RType;
class RFactory;

class RClassFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RFuncDeclVisitor& visitor) final;
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class REClassFuncDecl : public RClassFuncDecl
{
    EClassFuncDecl* decl;
};


} // namespace Citron
