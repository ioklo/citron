#pragma once

#include "RSymbol.h"
#include "RBodyOuter.h"
#include "RFunc.h"
#include "RStructConstructorDecl.h"
#include "RSymbolComponent.h" 
#include "RStruct.h"

namespace Citron
{

class RStructConstructor 
    : public RSymbol
    , public RBodyOuter
    , public RFunc
    , private RSymbolComponent<RStruct, RStructConstructorDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}