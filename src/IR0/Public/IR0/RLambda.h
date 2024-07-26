#pragma once

#include "RSymbol.h"
#include "RBodyOuter.h"
#include "RFunc.h"
#include "RSymbolComponent.h"
#include "RLambdaDecl.h"
#include "RBodyOuter.h"

namespace Citron
{

class RLambda 
    : public RSymbol
    , public RBodyOuter
    , public RFunc
    , private RSymbolComponent<RBodyOuter, RLambdaDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}