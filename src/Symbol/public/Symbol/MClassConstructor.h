#pragma once

#include "MSymbol.h"
#include "MBodyOuter.h"
#include "MFunc.h"
#include "MClassConstructorDecl.h"
#include "MSymbolComponent.h" 
#include "MClass.h"

namespace Citron
{

class MClassConstructor 
    : public MSymbol
    , public MBodyOuter
    , public MFunc
    , private MSymbolComponent<MClass, MClassConstructorDecl>
{   
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}