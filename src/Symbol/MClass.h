#pragma once

#include "MSymbol.h"
#include "MTypeOuter.h"
#include "MClassDecl.h"
#include "MTypeOuter.h"
#include "MSymbolComponent.h"

namespace Citron
{

class MClass 
    : public MSymbol
    , public MTypeOuter
    , private MSymbolComponent<MTypeOuter, MClassDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this);  }
    void Accept(MTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
};

using MClassPtr = std::shared_ptr<MClass>;

}