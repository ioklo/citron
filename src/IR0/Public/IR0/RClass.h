#pragma once

#include "RSymbol.h"
#include "RTypeOuter.h"
#include "RClassDecl.h"
#include "RTypeOuter.h"
#include "RType.h"
#include "RSymbolComponent.h"

namespace Citron
{

class RClass 
    : public RSymbol
    , public RTypeOuter
    , public RType
    , private RSymbolComponent<RTypeOuter, RClassDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this);  }
    void Accept(RTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Class; }
};

}