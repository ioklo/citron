export module Citron.Symbols:MGlobalFuncDecl;

import <vector>;
import <optional>;
import <memory>;

import :MAccessor;
import :MNames;
import :MFuncReturn;
import :MFuncParameter;
import :MTopLevelOuterDecl;

namespace Citron {

export class MGlobalFuncDecl
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