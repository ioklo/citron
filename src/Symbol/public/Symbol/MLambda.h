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
    template<typename TResult>
    TResult Accept(MSymbolVisitor<TResult>& visitor) override { return visitor.Visit(*this); }
    void Accept(MBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}