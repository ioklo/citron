#pragma once

#include "RSymbol.h"
#include "RSymbolComponent.h"
#include "RLambda.h"
#include "RLambdaMemberVarDecl.h"

namespace Citron
{

class RLambdaMemberVar 
    : public RSymbol
    , private RSymbolComponent<RLambda, RLambdaMemberVarDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
};


}

