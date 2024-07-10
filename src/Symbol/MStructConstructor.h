#pragma once

#include "MStructConstructorDecl.h"
#include "MSymbolComponent.h" 
#include "MStruct.h"

namespace Citron
{

class MStructConstructor : private MSymbolComponent<MStruct, MStructConstructorDecl>
{
};

}