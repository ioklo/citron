#pragma once
#include "RSymbolConfig.h"

#include "RFuncDeclBase.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RThisKind.h"

namespace Citron {

class EGlobalFuncDecl;

class RType;
class RFactory;
class RTypeParamDecl;
class RTypeArguments;

// abstract
class RGlobalFuncDecl : public RFuncDeclBase
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REGlobalFuncDecl : public RGlobalFuncDecl
{
    EGlobalFuncDecl* externalFuncDecl;
};

}