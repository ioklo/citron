export module Citron.Symbols:MGlobalFunc;

import <vector>;
import :MGlobalFuncDecl;
import :MTypes;
import :MTopLevelOuter;

namespace Citron {

class MGlobalFunc
{
    MTopLevelOuter outer;
    MGlobalFuncDecl decl;
    std::vector<MType> typeArgs;
};

}