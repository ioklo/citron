#pragma once

#include "MSymbolComponent.h"
#include "MClass.h"
#include "MClassMemberFuncDecl.h"

namespace Citron
{

class MClassMemberFunc : private MSymbolComponent<MClass, MClassMemberFuncDecl>
{   
};

}