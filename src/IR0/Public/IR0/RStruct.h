#pragma once

#include "RStruct.h"
#include "RTypeOuter.h"
#include "RStructDecl.h"
#include "RSymbolComponent.h"
#include "RTypeOuter.h"

namespace Citron
{

class RStruct 
    : public RSymbol
    , public RTypeOuter
    , private RSymbolComponent<RTypeOuter, RStructDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}