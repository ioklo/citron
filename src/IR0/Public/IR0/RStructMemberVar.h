#pragma once

#include "RSymbol.h"
#include "RStruct.h"
#include "RStructMemberVarDecl.h"
#include "RSymbolComponent.h"

namespace Citron
{

class RStructMemberVar 
    : public RSymbol
    , private RSymbolComponent<RStruct, RStructMemberVarDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
};


}