#pragma once

#include "MSymbol.h"
#include "MBodyOuter.h"
#include "MFunc.h"
#include "MStructConstructorDecl.h"
#include "MSymbolComponent.h" 
#include "MStruct.h"

namespace Citron
{

class MStructConstructor 
    : public MSymbol
    , public MBodyOuter
    , public MFunc
    , private MSymbolComponent<MStruct, MStructConstructorDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}