#pragma once
#include "MClass.h"
#include "MClassMemberVarDecl.h"
#include "MSymbolComponent.h"

namespace Citron
{

class MClassMemberVar : private MSymbolComponent<MClass, MClassMemberVarDecl>
{

};


}