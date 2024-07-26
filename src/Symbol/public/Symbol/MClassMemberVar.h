#pragma once

#include "MSymbol.h"
#include "MClass.h"
#include "MClassMemberVarDecl.h"
#include "MSymbolComponent.h"

namespace Citron
{

class MClassMemberVar 
    : public MSymbol
    , private MSymbolComponent<MClass, MClassMemberVarDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
};


}