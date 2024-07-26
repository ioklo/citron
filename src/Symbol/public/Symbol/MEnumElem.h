#pragma once

#include "MSymbol.h"
#include "MSymbolComponent.h"
#include "MEnumElemDecl.h"

namespace Citron
{
class MEnum;

class MEnumElem 
    : public MSymbol
    , private MSymbolComponent<MEnum, MEnumElemDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
};

}