#pragma once

#include "MEnumDecl.h"
#include "MSymbolComponent.h"
#include "MTypeOuter.h"

namespace Citron
{

class MEnum : private MSymbolComponent<MTypeOuter, MEnumDecl>
{

};

}