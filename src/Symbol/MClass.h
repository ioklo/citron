#pragma once

#include "MClassDecl.h"
#include "MTypeOUter.h"
#include "MSymbolComponent.h"

namespace Citron
{

class MClass : private MSymbolComponent<MTypeOuter, MClassDecl>
{   
};

}