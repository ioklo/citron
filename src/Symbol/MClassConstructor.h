#pragma once

#include "MClassConstructorDecl.h"
#include "MSymbolComponent.h" 
#include "MClass.h"

namespace Citron
{

class MClassConstructor : private MSymbolComponent<MClass, MClassConstructorDecl>
{   
};

}