#pragma once

#include "RStruct.h"
#include "RTypeOuter.h"
#include "RType.h"
#include "RStructDecl.h"
#include "RSymbolComponent.h"
#include "RTypeOuter.h"

namespace Citron
{

class RStruct 
    : public RSymbol
    , public RTypeOuter
    , public RType
    , private RSymbolComponent<RTypeOuter, RStructDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
};

}