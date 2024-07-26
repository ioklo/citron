#pragma once

#include "MStruct.h"
#include "MTypeOuter.h"
#include "MStructDecl.h"
#include "MSymbolComponent.h"
#include "MTypeOuter.h"

namespace Citron
{

class MStruct 
    : public MSymbol
    , public MTypeOuter
    , private MSymbolComponent<MTypeOuter, MStructDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}