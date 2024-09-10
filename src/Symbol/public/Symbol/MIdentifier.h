#pragma once

#include <vector>
#include "MNames.h"

namespace Citron {

class MType;
using MTypePtr = std::shared_ptr<MType>;

struct MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypePtr> paramIds;
};

}