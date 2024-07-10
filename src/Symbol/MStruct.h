#pragma once

#include "MStructDecl.h"
#include "MSymbolComponent.h"
#include "MTypeOuter.h"

namespace Citron
{

class MStruct : private MSymbolComponent<MTypeOuter, MStructDecl>
{
};

}