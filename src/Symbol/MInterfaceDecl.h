#pragma once

#include "MTypeDeclOuter.h"
#include "MAccessor.h"
#include "MNames.h"

namespace Citron
{

class MInterfaceDecl
{
    MTypeDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;
};

}