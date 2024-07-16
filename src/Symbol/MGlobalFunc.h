#pragma once

#include <vector>

#include "MSymbol.h"
#include "MBodyOuter.h"
#include "MFunc.h"
#include "MGlobalFuncDecl.h"
#include "MType.h"
#include "MTopLevelOuter.h"

namespace Citron {

class MGlobalFunc
    : public MSymbol
    , public MBodyOuter
    , public MFunc
    , private MSymbolComponent<MTopLevelOuter, MGlobalFuncDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncVisitor& visitor) override { visitor.Visit(*this); }
};

using MGlobalFuncPtr = std::shared_ptr<MGlobalFunc>;

}