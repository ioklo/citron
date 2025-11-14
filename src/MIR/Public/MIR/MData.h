#pragma once
#include <vector>
#include <span>
#include "MFuncBody.h"

namespace Citron {

class NFuncDecl;

class MData
{   
    std::vector<MFuncBody> funcBodies;

public:
    std::span<MFuncBody> GetAllFuncBodies() { return funcBodies; }
};

} // namespace Citron