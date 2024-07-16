#pragma once

#include "MSymbol.h"
#include "MEnumElem.h"
#include "MEnumElemMemberVarDecl.h"

namespace Citron
{

class MEnumElemMemberVar 
    : public MSymbol
    , private MSymbolComponent<MEnumElem, MEnumElemMemberVarDecl>
{
public:
    void Accept(MSymbolVisitor& visitor) override { visitor.Visit(*this); }
};

using MEnumElemMemberVarPtr = std::shared_ptr<MEnumElemMemberVar>;


}