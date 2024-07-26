#pragma once

#include "RSymbol.h"
#include "REnumDecl.h"
#include "RSymbolComponent.h"
#include "RTypeOuter.h"

namespace Citron
{

class REnum 
    : public RSymbol
    , private RSymbolComponent<RTypeOuter, REnumDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
};

}