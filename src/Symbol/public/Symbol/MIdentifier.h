#pragma once

#include "MNames.h"

namespace Citron {

class MTypeId;
using MTypeIdPtr = std::shared_ptr<MTypeId>;

struct MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypeIdPtr> paramIds;
};

}