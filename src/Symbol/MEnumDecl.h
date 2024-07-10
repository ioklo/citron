#pragma once

#include "MAccessor.h"
#include "MNames.h"
#include "MEnumElemDecl.h"
#include "MTypeDeclOuter.h"

namespace Citron
{

class MEnumDecl
{
    MTypeDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::optional<std::vector<std::shared_ptr<MEnumElemDecl>>> elems; // lazy initialization

    // std::unordered_map<std::string, int> elemsByName;
};

}

