#pragma once
#include "MStruct.h"
#include "MStructMemberVarDecl.h"
#include "MSymbolComponent.h"

namespace Citron
{

class MStructMemberVar : private MSymbolComponent<MStruct, MStructMemberVarDecl>
{

};


}