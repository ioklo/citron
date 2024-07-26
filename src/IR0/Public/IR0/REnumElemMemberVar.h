#pragma once

#include "RSymbol.h"
#include "REnumElem.h"
#include "REnumElemMemberVarDecl.h"

namespace Citron
{

class REnumElemMemberVar 
    : public RSymbol
    , private RSymbolComponent<REnumElem, REnumElemMemberVarDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
};


}