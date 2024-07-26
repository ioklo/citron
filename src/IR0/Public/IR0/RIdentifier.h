#pragma once

#include <memory>
#include <vector>

#include "RNames.h"

namespace Citron {

class RTypeId;
using RTypeIdPtr = std::shared_ptr<RTypeId>;

struct RIdentifier
{
    RName name;
    int typeParamCount;
    std::vector<RTypeIdPtr> paramIds;
};

}