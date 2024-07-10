#pragma once

#include "MGlobalFuncDecl.h"
#include "MTypes.h"
#include "MTopLevelOuter.h"

namespace Citron {

class MGlobalFunc
{
    MTopLevelOuter outer;
    MGlobalFuncDecl decl;
    std::vector<MType> typeArgs;
};

}