#pragma once

#include "MSymbol.h"
#include "MBodyOuter.h"
#include "MFunc.h"
#include "MSymbolComponent.h"
#include "MLambdaDecl.h"
#include "MBodyOuter.h"

namespace Citron
{

class MLambda 
    : public MSymbol
    , public MBodyOuter
    , public MFunc
    , private MSymbolComponent<MBodyOuter, MLambdaDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncVisitor& visitor) override { visitor.Visit(*this); }
};

using MLambdaPtr = std::shared_ptr<MLambda>;

}