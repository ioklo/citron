#pragma once

#include "MSymbol.h"
#include "MEnumDecl.h"
#include "MSymbolComponent.h"
#include "MTypeOuter.h"

namespace Citron
{

class MEnum 
    : public MSymbol
    , private MSymbolComponent<MTypeOuter, MEnumDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
};

}