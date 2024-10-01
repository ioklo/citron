#include "RLoc.h"
#include "RExp.h"

namespace Citron {

RTypePtr RTempLoc::GetType(RTypeFactory& factory)
{
    return exp->GetType(factory);
}


}