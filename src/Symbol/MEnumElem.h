#pragma once
#include "MSymbolComponent.h"
#include "MEnumElemDecl.h"

namespace Citron
{

class MEnum;

class MEnumElem : private MSymbolComponent<std::shared_ptr<MEnum>, MEnumElemDecl>
{

};

}