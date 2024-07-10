#pragma once

#include "MSymbolComponent.h"
#include "MStruct.h"
#include "MStructMemberFuncDecl.h"

namespace Citron
{

class MStructMemberFunc : private MSymbolComponent<MStruct, MStructMemberFuncDecl>
{
};

}