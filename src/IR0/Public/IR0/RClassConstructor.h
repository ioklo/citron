#pragma once

#include "RSymbol.h"
#include "RBodyOuter.h"
#include "RFunc.h"
#include "RClassConstructorDecl.h"
#include "RSymbolComponent.h" 
#include "RClass.h"

namespace Citron
{

class RClassConstructor 
    : public RSymbol
    , public RBodyOuter
    , public RFunc
    , private RSymbolComponent<RClass, RClassConstructorDecl>
{   
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}