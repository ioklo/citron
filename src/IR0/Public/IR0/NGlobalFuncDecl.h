#pragma once

#include <vector>
#include <optional>

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "NTopLevelDeclOuter.h"
#include "NCommonFuncDeclComponent.h"
#include "RGlobalFuncDecl.h"

namespace Citron {

class MGlobalFuncDecl;

class NGlobalFuncDecl
    : public RGlobalFuncDecl
    , private NCommonFuncDeclComponent
{   
    struct FuncReturnAndParams
    {
        RFuncReturn funcReturn;
        std::vector<RFuncParameter> parameters;
    };

    NTopLevelDeclOuterWPtr outer;
    RAccessor accessor;    
    RName name;
    std::vector<RName> typeParams;

    std::optional<FuncReturnAndParams> funcReturnAndParams;
};

}