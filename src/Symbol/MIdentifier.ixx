module;
#include "Macros.h"

export module Citron.Symbols:MIdentifier;

import <vector>;

import :MNames;
import :MTypeId;

namespace Citron {

export class MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypeId> paramIds;

public:
    // MIdentifier(MName&& name, int typeParamCount, std::vector<MTypeId>&& paramIds);
    // DECLARE_DEFAULTS(MIdentifier)
};

}