#pragma once

#include "RSymbol.h"
#include "RBodyOuter.h"
#include "RFunc.h"
#include "RSymbolComponent.h"
#include "RClass.h"
#include "RClassMemberFuncDecl.h"

namespace Citron
{

class RClassMemberFunc 
    : public RSymbol
    , public RBodyOuter
    , public RFunc
    , private RSymbolComponent<RClass, RClassMemberFuncDecl>
{   
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}