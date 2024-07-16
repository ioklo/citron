#pragma once

#include "MSymbol.h"
#include "MStruct.h"
#include "MStructMemberVarDecl.h"
#include "MSymbolComponent.h"

namespace Citron
{

class MStructMemberVar 
    : public MSymbol
    , private MSymbolComponent<MStruct, MStructMemberVarDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
};

using MStructMemberVarPtr = std::shared_ptr<MStructMemberVar>;


}