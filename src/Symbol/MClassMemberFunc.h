#pragma once

#include "MSymbol.h"
#include "MBodyOuter.h"
#include "MFunc.h"
#include "MSymbolComponent.h"
#include "MClass.h"
#include "MClassMemberFuncDecl.h"

namespace Citron
{

class MClassMemberFunc 
    : public MSymbol
    , public MBodyOuter
    , public MFunc
    , private MSymbolComponent<MClass, MClassMemberFuncDecl>
{   
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncVisitor& visitor) override { visitor.Visit(*this); }
};

using MClassMemberFuncPtr = std::shared_ptr<MClassMemberFunc>;

}