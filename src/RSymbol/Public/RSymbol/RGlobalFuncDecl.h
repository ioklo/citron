#pragma once
#include "RSymbolConfig.h"

#include "RFuncDecl.h"

namespace Citron {

class EGlobalFuncDecl;

class RType;
class RFactory;

// abstract
class RGlobalFuncDecl
    : public RDecl
    , public RFuncDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RFuncDeclVisitor& visitor) final;
};

class REGlobalFuncDecl : public RGlobalFuncDecl
{
    EGlobalFuncDecl* externalFuncDecl;
};

}