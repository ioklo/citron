#pragma once

#include "RSymbol.h"
#include "RSymbolComponent.h"
#include "REnumElemDecl.h"

namespace Citron
{
class REnum;

class REnumElem 
    : public RSymbol
    , private RSymbolComponent<REnum, REnumElemDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
};

}