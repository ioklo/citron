#pragma once

#include "RSymbol.h"
#include "REnumDecl.h"
#include "RSymbolComponent.h"
#include "RType.h"
#include "RTypeOuter.h"

namespace Citron
{

class REnum 
    : public RSymbol
    , public RType
    , private RSymbolComponent<RTypeOuter, REnumDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Enum; }
};

}