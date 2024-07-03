export module Citron.Symbols:MGlobalFunc;

import "std.h";

import :MGlobalFuncDecl;
import :MTypes;
import :MTopLevelOuter;

namespace Citron {

export class MGlobalFunc
{
    MTopLevelOuter outer;
    MGlobalFuncDecl decl;
    std::vector<MType> typeArgs;
};

}