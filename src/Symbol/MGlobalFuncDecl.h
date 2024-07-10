#pragma once

#include "MAccessor.h"
#include "MNames.h"
#include "MFuncReturn.h"
#include "MFuncParameter.h"
#include "MTopLevelOuterDecl.h"

namespace Citron {

class MGlobalFuncDecl
{   
    struct FuncReturnAndParams
    {
        MFuncReturn funcReturn;
        std::vector<MFuncParameter> parameters;
    };

    MTopLevelOuterDecl outer;
    MAccessor accessor;    
    MName name;
    std::vector<MName> typeParams;

    std::optional<FuncReturnAndParams> funcReturnAndParams;
};

}