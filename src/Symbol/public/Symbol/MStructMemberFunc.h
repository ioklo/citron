#pragma once

#include "MSymbol.h"
#include "MBodyOuter.h"
#include "MFunc.h"
#include "MSymbolComponent.h"
#include "MStruct.h"
#include "MStructMemberFuncDecl.h"

namespace Citron
{

class MStructMemberFunc 
    : public MSymbol
    , public MBodyOuter
    , public MFunc
    , private MSymbolComponent<MStruct, MStructMemberFuncDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncVisitor& visitor) override { visitor.Visit(*this); }
};

}