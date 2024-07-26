#pragma once

#include "RSymbol.h"
#include "RClass.h"
#include "RClassMemberVarDecl.h"
#include "RSymbolComponent.h"

namespace Citron
{

class RClassMemberVar 
    : public RSymbol
    , private RSymbolComponent<RClass, RClassMemberVarDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
};


}