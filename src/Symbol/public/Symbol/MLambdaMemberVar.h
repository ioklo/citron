#pragma once

#include "MSymbol.h"
#include "MSymbolComponent.h"
#include "MLambda.h"
#include "MLambdaMemberVarDecl.h"

namespace Citron
{

class MLambdaMemberVar 
    : public MSymbol
    , private MSymbolComponent<MLambda, MLambdaMemberVarDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
};


}

