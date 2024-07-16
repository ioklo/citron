#pragma once
#include "SymbolMacros.h"

#include "MNames.h"

namespace Citron {

class MTypeId;

class MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<std::shared_ptr<MTypeId>> paramIds;

public:
    // MIdentifier(MName&& name, int typeParamCount, std::vector<MTypeId>&& paramIds);
    // DECLARE_DEFAULTS(MIdentifier)
};

}