export module Citron.Symbols:MInterfaceDecl;

import "std.h";

import :MTypeDeclOuter;
import :MAccessor;
import :MNames;

namespace Citron
{

class MInterfaceDecl
{
    MTypeDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;
};

}