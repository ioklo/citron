export module Citron.Symbols:MFunc;

import "std.h";

import :MGlobalFunc;

import :MClassConstructor;
import :MClassMemberFunc;

import :MStructConstructor;
import :MStructMemberFunc;

import :MLambda;

namespace Citron
{

export using MFunc = std::variant<
    MGlobalFunc,        // top-level decl space

    MClassConstructor,  // construct decl space
    MClassMemberFunc,   // construct decl space

    MStructConstructor, // struct decl space
    MStructMemberFunc,  // struct decl space
    
    MLambda             // body space
>;


}