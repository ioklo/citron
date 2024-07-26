#pragma once

#include <vector>

#include "RSymbol.h"
#include "RBodyOuter.h"
#include "RFunc.h"
#include "RGlobalFuncDecl.h"
#include "RType.h"
#include "RTopLevelOuter.h"
#include "RSymbolComponent.h"

namespace Citron {

class RGlobalFunc
    : public RSymbol
    , public RBodyOuter
    , public RFunc
    , private RSymbolComponent<RTopLevelOuter, RGlobalFuncDecl>
{
public:
    void Accept(RSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}