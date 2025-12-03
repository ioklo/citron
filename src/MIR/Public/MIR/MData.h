#pragma once
#include "MIRConfig.h"

#include <vector>
#include <span>
#include "MFuncBody.h"

namespace Citron {

class NFuncDecl;

class MData
{   
    std::vector<MFuncBody> funcBodies;

public:
    MIR_API MData(std::vector<MFuncBody>&& funcBodies);
    std::span<MFuncBody> GetAllFuncBodies() { return funcBodies; }
};

} // namespace Citron