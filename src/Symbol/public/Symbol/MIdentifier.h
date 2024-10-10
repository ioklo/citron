#pragma once

#include <vector>
#include "MNames.h"

namespace Citron {

using MTypePtr = std::shared_ptr<class MType>;

struct MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypePtr> paramIds;
};

}