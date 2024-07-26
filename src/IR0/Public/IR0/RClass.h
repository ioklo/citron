#pragma once

#include "RSymbol.h"
#include "RTypeOuter.h"
#include "RClassDecl.h"
#include "RTypeOuter.h"
#include "RSymbolComponent.h"

namespace Citron
{

class RClass 
    : public RSymbol
    , public RTypeOuter
    , private RSymbolComponent<RTypeOuter, RClassDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this);  }
    void Accept(RTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}