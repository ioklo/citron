#pragma once
#include "SymbolMacros.h"

#include "MNames.h"
#include "MTypeId.h"

namespace Citron {

class MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypeId> paramIds;

public:
    // MIdentifier(MName&& name, int typeParamCount, std::vector<MTypeId>&& paramIds);
    // DECLARE_DEFAULTS(MIdentifier)
};

}